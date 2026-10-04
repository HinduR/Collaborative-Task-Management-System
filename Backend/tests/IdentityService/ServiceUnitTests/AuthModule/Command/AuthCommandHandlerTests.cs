using FluentAssertions;
using IdentityService.Application.AuthenticationModule.Command;
using IdentityService.Application.AuthenticationModule.Command.Logout;
using IdentityService.Application.AuthenticationModule.Command.Refresh;
using IdentityService.Application.AuthenticationModule.Contract.IService;
using IdentityService.Application.AuthenticationModule.Dto;
using Moq;
using Shared.Logging.Contracts;
using Xunit;

namespace IdentityService.UnitTests.AuthModule.Command;

public class AuthCommandHandlerTests
{
    [Fact]
    public async Task RefreshAccessTokenHandler_ShouldDelegateToLoginService()
    {
        Guid refreshToken = Guid.NewGuid();
        var expected = TokenResponse();
        var loginService = new Mock<ILoginService>();
        loginService.Setup(x => x.RefreshAccessTokenAsync(refreshToken, CancellationToken.None)).ReturnsAsync(expected);
        var handler = new RefreshAccessTokenCommandHandler(loginService.Object);

        TokenResponseDto actual = await handler.Handle(
            new RefreshAccessTokenCommand(new RefreshAccessTokenRequest { RefreshToken = refreshToken }),
            CancellationToken.None);

        actual.Should().BeSameAs(expected);
    }

    [Fact]
    public void RefreshAccessTokenValidator_ShouldFail_WhenRefreshTokenIsEmpty()
    {
        var result = new RefreshAccessTokenCommandValidator().Validate(
            new RefreshAccessTokenCommand(new RefreshAccessTokenRequest()));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage == "Refresh token is required.");
    }

    [Fact]
    public async Task LogoutHandler_ShouldDelegateToLoginService()
    {
        Guid refreshToken = Guid.NewGuid();
        var loginService = new Mock<ILoginService>();
        var handler = new LogoutCommandHandler(loginService.Object);

        await handler.Handle(new LogoutCommand(refreshToken), CancellationToken.None);

        loginService.Verify(x => x.LogoutAsync(refreshToken, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task CreateGoogleLoginCodeHandler_ShouldDelegateToLoginService()
    {
        var user = new UserDto { GoogleSubjectId = "google-id", Email = "ada@example.com", Name = "Ada" };
        var loginService = new Mock<ILoginService>();
        loginService.Setup(x => x.CreateGoogleLoginCodeAsync(user, CancellationToken.None)).ReturnsAsync("code");
        var handler = new CreateGoogleLoginCodeCommandHandler(loginService.Object);

        string actual = await handler.Handle(new CreateGoogleLoginCodeCommand(user), CancellationToken.None);

        actual.Should().Be("code");
    }

    [Fact]
    public void CreateGoogleLoginCodeValidator_ShouldFail_ForMissingGoogleIdOrInvalidEmail()
    {
        var result = new CreateGoogleLoginCodeCommandValidator().Validate(
            new CreateGoogleLoginCodeCommand(new UserDto { Email = "bad-email" }));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage == "Google subject identifier is required.");
        result.Errors.Should().Contain(x => x.ErrorMessage == "A valid email address is required.");
    }

    [Fact]
    public async Task ExchangeGoogleLoginCodeHandler_ShouldDelegateToLoginService()
    {
        var expected = TokenResponse();
        var loginService = new Mock<ILoginService>();
        var logger = new Mock<ILoggerManager<ExchangeGoogleLoginCodeCommandHandler>>();
        loginService.Setup(x => x.ExchangeGoogleLoginCodeAsync("code", CancellationToken.None)).ReturnsAsync(expected);
        var handler = new ExchangeGoogleLoginCodeCommandHandler(loginService.Object, logger.Object);

        TokenResponseDto actual = await handler.Handle(
            new ExchangeGoogleLoginCodeCommand(new GoogleLoginCodeExchangeRequest { Code = "code" }),
            CancellationToken.None);

        actual.Should().BeSameAs(expected);
    }

    [Fact]
    public void ExchangeGoogleLoginCodeValidator_ShouldFail_WhenCodeIsMissing()
    {
        var result = new ExchangeGoogleLoginCodeCommandValidator().Validate(
            new ExchangeGoogleLoginCodeCommand(new GoogleLoginCodeExchangeRequest()));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage == "Login code is required.");
    }

    private static TokenResponseDto TokenResponse()
    {
        return new TokenResponseDto
        {
            AccessToken = "access-token",
            RefreshToken = Guid.NewGuid(),
            ExpiresIn = 3600
        };
    }
}
