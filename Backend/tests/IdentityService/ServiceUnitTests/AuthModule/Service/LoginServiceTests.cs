using System.Linq.Expressions;
using FluentAssertions;
using IdentityService.Application.AuthenticationModule.Contract.IRepository;
using IdentityService.Application.AuthenticationModule.Contract.IService;
using IdentityService.Application.AuthenticationModule.Dto;
using IdentityService.Application.AuthenticationModule.Service;
using IdentityService.Application.Common;
using IdentityService.Application.RoleModule.Contract.IRepository;
using IdentityService.Application.UserModule.Contract.IRepository;
using IdentityService.Domain.Models;
using Microsoft.Extensions.Configuration;
using MockQueryable;
using Moq;
using Shared.Common.contracts;
using Shared.Cryptography.Application.Cryptography.Contract;
using Shared.Exceptions.Infrastructure;
using Shared.Logging.Contracts;
using Xunit;

namespace IdentityService.UnitTests.AuthModule.Service;

public class LoginServiceTests
{
    [Fact]
    public async Task CreateGoogleLoginCodeAsync_ShouldValidateUserCreateCodeAndSetContext()
    {
        Guid userId = Guid.NewGuid();
        var user = new User { Id = userId, Name = "Ada", Email = "ada@example.com", IsActive = true };
        var repo = RepoWrapper();
        var userContext = new Mock<IUserContext>();
        var codeService = new Mock<IGoogleLoginCodeService>();
        codeService.Setup(x => x.CreateCode(It.Is<GoogleLoginCodeCacheDto>(dto =>
                dto.UserId == userId && dto.GoogleSubjectId == "google-id")))
            .Returns("code");
        repo.UserRepository.Setup(x => x.FindFirstByConditionAsync(It.IsAny<Expression<Func<User, bool>>>(), CancellationToken.None))
            .ReturnsAsync(user);
        repo.RefreshTokenRepository.Setup(x => x.FindFirstByConditionAsync(It.IsAny<Expression<Func<RefreshToken, bool>>>(), CancellationToken.None))
            .ReturnsAsync((RefreshToken?)null);

        LoginService service = CreateService(repo, userContext, codeService.Object);

        string result = await service.CreateGoogleLoginCodeAsync(
            new UserDto { GoogleSubjectId = " google-id ", Email = " ADA@example.com " },
            CancellationToken.None);

        result.Should().Be("code");
        userContext.Verify(x => x.SetUserId(userId), Times.Once);
    }

    [Fact]
    public async Task CreateGoogleLoginCodeAsync_ShouldThrow_WhenUserIsNotConfiguredOrGoogleSubjectDiffers()
    {
        var repo = RepoWrapper();
        LoginService service = CreateService(repo, new Mock<IUserContext>(), Mock.Of<IGoogleLoginCodeService>());

        await service.Invoking(x => x.CreateGoogleLoginCodeAsync(
                new UserDto { GoogleSubjectId = "google-id", Email = "ada@example.com" },
                CancellationToken.None))
            .Should().ThrowAsync<ForBiddenCustomException>();

        var user = new User { Id = Guid.NewGuid(), Name = "Ada", Email = "ada@example.com", IsActive = true };
        repo.UserRepository.Setup(x => x.FindFirstByConditionAsync(It.IsAny<Expression<Func<User, bool>>>(), CancellationToken.None))
            .ReturnsAsync(user);
        repo.RefreshTokenRepository.Setup(x => x.FindFirstByConditionAsync(It.IsAny<Expression<Func<RefreshToken, bool>>>(), CancellationToken.None))
            .ReturnsAsync(new RefreshToken
            {
                UserId = user.Id,
                GoogleSubjectId = "other-google-id",
                TokenHash = "hash"
            });

        await service.Invoking(x => x.CreateGoogleLoginCodeAsync(
                new UserDto { GoogleSubjectId = "google-id", Email = "ada@example.com" },
                CancellationToken.None))
            .Should().ThrowAsync<ForBiddenCustomException>();
    }

    [Fact]
    public async Task ExchangeGoogleLoginCodeAsync_ShouldThrow_WhenCodeIsMissingOrInvalid()
    {
        LoginService service = CreateService(RepoWrapper(), new Mock<IUserContext>(), Mock.Of<IGoogleLoginCodeService>());

        await service.Invoking(x => x.ExchangeGoogleLoginCodeAsync("", CancellationToken.None))
            .Should().ThrowAsync<UnAuthorizedCustomException>();
        await service.Invoking(x => x.ExchangeGoogleLoginCodeAsync("missing", CancellationToken.None))
            .Should().ThrowAsync<UnAuthorizedCustomException>();
    }

    [Fact]
    public async Task RefreshAccessTokenAsync_ShouldThrow_WhenRefreshTokenIsInvalid()
    {
        LoginService service = CreateService(RepoWrapper(), new Mock<IUserContext>(), Mock.Of<IGoogleLoginCodeService>());

        await service.Invoking(x => x.RefreshAccessTokenAsync(Guid.NewGuid(), CancellationToken.None))
            .Should().ThrowAsync<BadRequestCustomException>();
    }

    [Fact]
    public async Task LogoutAsync_ShouldBeIdempotentForUnknownToken()
    {
        var repo = RepoWrapper();
        LoginService service = CreateService(repo, new Mock<IUserContext>(), Mock.Of<IGoogleLoginCodeService>());

        await service.LogoutAsync(Guid.NewGuid(), CancellationToken.None);

        repo.Wrapper.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RefreshAccessTokenAsync_ShouldRevokeOldTokenCreateNewTokenAndSave()
    {
        Guid userId = Guid.NewGuid();
        Guid refreshToken = Guid.NewGuid();
        Guid roleId = Guid.NewGuid();
        var savedRefreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            GoogleSubjectId = "google-id",
            TokenHash = "hash",
            ExpiryAt = DateTime.Now.AddMinutes(5),
            IsActive = true
        };
        var user = new User { Id = userId, Name = "Ada", Email = "ada@example.com", IsActive = true };
        var repo = RepoWrapper();
        var userContext = new Mock<IUserContext>();

        SetupTokenUser(repo, user, roleId);
        repo.RefreshTokenRepository
            .SetupSequence(x => x.FindFirstByConditionAsync(It.IsAny<Expression<Func<RefreshToken, bool>>>(), CancellationToken.None))
            .ReturnsAsync(savedRefreshToken);

        LoginService service = CreateService(repo, userContext, Mock.Of<IGoogleLoginCodeService>(), JwtConfiguration());

        TokenResponseDto result = await service.RefreshAccessTokenAsync(refreshToken, CancellationToken.None);

        result.AccessToken.Should().NotBeEmpty();
        result.RefreshToken.Should().NotBeEmpty();
        result.ExpiresIn.Should().Be(900);
        savedRefreshToken.IsActive.Should().BeFalse();
        repo.RefreshTokenRepository.Verify(x => x.CreateAsync(
            It.Is<RefreshToken>(token => token.UserId == userId && token.GoogleSubjectId == "google-id" && token.IsActive),
            CancellationToken.None), Times.Once);
        userContext.Verify(x => x.SetUserId(userId), Times.Once);
        repo.Wrapper.Verify(x => x.SaveChangesAsync(CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task ExchangeGoogleLoginCodeAsync_ShouldConsumeCodeCreateTokenAndSave()
    {
        Guid userId = Guid.NewGuid();
        Guid roleId = Guid.NewGuid();
        var user = new User { Id = userId, Name = "Ada", Email = "ada@example.com", IsActive = true };
        var repo = RepoWrapper();
        var userContext = new Mock<IUserContext>();
        var codeService = new Mock<IGoogleLoginCodeService>();

        codeService.Setup(x => x.ConsumeCode("code"))
            .Returns(new GoogleLoginCodeCacheDto { UserId = userId, GoogleSubjectId = "google-id" });
        SetupTokenUser(repo, user, roleId);

        LoginService service = CreateService(repo, userContext, codeService.Object, JwtConfiguration());

        TokenResponseDto result = await service.ExchangeGoogleLoginCodeAsync("code", CancellationToken.None);

        result.AccessToken.Should().NotBeEmpty();
        repo.RefreshTokenRepository.Verify(x => x.CreateAsync(
            It.Is<RefreshToken>(token => token.UserId == userId && token.GoogleSubjectId == "google-id"),
            CancellationToken.None), Times.Once);
        userContext.Verify(x => x.SetUserId(userId), Times.Once);
        repo.Wrapper.Verify(x => x.SaveChangesAsync(CancellationToken.None), Times.Once);
    }

    private static LoginService CreateService(
        RepoMocks repo,
        Mock<IUserContext> userContext,
        IGoogleLoginCodeService codeService)
    {
        return CreateService(repo, userContext, codeService, new ConfigurationBuilder().Build());
    }

    private static LoginService CreateService(
        RepoMocks repo,
        Mock<IUserContext> userContext,
        IGoogleLoginCodeService codeService,
        IConfiguration configuration)
    {
        var authenticity = new Mock<IAuthenticity>();
        authenticity.Setup(x => x.Hash(It.IsAny<string>(), It.IsAny<System.Security.Cryptography.HashAlgorithm>()))
            .Returns<string, System.Security.Cryptography.HashAlgorithm>((value, _) => $"hash:{value}");

        return new LoginService(
            repo.Wrapper.Object,
            configuration,
            authenticity.Object,
            userContext.Object,
            new JwtService(configuration),
            codeService,
            Mock.Of<ILoggerManager<LoginService>>());
    }

    private static void SetupTokenUser(
        RepoMocks repo,
        User user,
        Guid roleId)
    {
        repo.UserRepository.Setup(x => x.FindFirstByConditionAsync(It.IsAny<Expression<Func<User, bool>>>(), CancellationToken.None))
            .ReturnsAsync(user);
        repo.UserRoleMappingRepository.Setup(x => x.FindFirstByConditionAsync(It.IsAny<Expression<Func<UserRoleMapping, bool>>>(), CancellationToken.None))
            .ReturnsAsync(new UserRoleMapping { Id = Guid.NewGuid(), UserId = user.Id, RoleId = roleId, IsActive = true });
        repo.RoleRepository.Setup(x => x.FindFirstByConditionAsync(It.IsAny<Expression<Func<Role, bool>>>(), CancellationToken.None))
            .ReturnsAsync(new Role { Id = roleId, Name = "Admin", IsActive = true });
        repo.RoleFeatureMappingRepository.Setup(x => x.FindByCondition(It.IsAny<Expression<Func<RoleFeatureMapping, bool>>>()))
            .Returns(new List<RoleFeatureMapping>().BuildMock());
    }

    private static IConfiguration JwtConfiguration()
    {
        using System.Security.Cryptography.RSA rsa = System.Security.Cryptography.RSA.Create(2048);

        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["JwtConfig:PrivateKey"] = rsa.ExportPkcs8PrivateKeyPem(),
                ["JwtConfig:PublicKey"] = rsa.ExportSubjectPublicKeyInfoPem(),
                ["JwtConfig:Issuer"] = "round-table",
                ["JwtConfig:Audience"] = "round-table-client",
                ["JwtConfig:DurationInMinutes"] = "15",
                ["JwtConfig:RefreshDurationInMinutes"] = "60"
            })
            .Build();
    }

    private static RepoMocks RepoWrapper()
    {
        var wrapper = new Mock<IRepoWrapper>();
        var userRepository = new Mock<IUserRepository>();
        var roleRepository = new Mock<IRoleRepository>();
        var featureRepository = new Mock<IFeatureRepository>();
        var userRoleMappingRepository = new Mock<IUserRoleMappingRepository>();
        var roleFeatureMappingRepository = new Mock<IRoleFeatureMappingRepository>();
        var refreshTokenRepository = new Mock<IRefreshTokenRepository>();

        wrapper.SetupGet(x => x.UserRepository).Returns(userRepository.Object);
        wrapper.SetupGet(x => x.RoleRepository).Returns(roleRepository.Object);
        wrapper.SetupGet(x => x.FeatureRepository).Returns(featureRepository.Object);
        wrapper.SetupGet(x => x.UserRoleMappingRepository).Returns(userRoleMappingRepository.Object);
        wrapper.SetupGet(x => x.RoleFeatureMappingRepository).Returns(roleFeatureMappingRepository.Object);
        wrapper.SetupGet(x => x.RefreshTokenRepository).Returns(refreshTokenRepository.Object);

        return new RepoMocks(
            wrapper,
            userRepository,
            roleRepository,
            featureRepository,
            userRoleMappingRepository,
            roleFeatureMappingRepository,
            refreshTokenRepository);
    }

    private sealed record RepoMocks(
        Mock<IRepoWrapper> Wrapper,
        Mock<IUserRepository> UserRepository,
        Mock<IRoleRepository> RoleRepository,
        Mock<IFeatureRepository> FeatureRepository,
        Mock<IUserRoleMappingRepository> UserRoleMappingRepository,
        Mock<IRoleFeatureMappingRepository> RoleFeatureMappingRepository,
        Mock<IRefreshTokenRepository> RefreshTokenRepository);
}
