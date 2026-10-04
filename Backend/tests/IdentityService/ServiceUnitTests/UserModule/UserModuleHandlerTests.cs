using FluentAssertions;
using IdentityService.Application.UserModule.Command.AssignRole;
using IdentityService.Application.UserModule.Contract.IService;
using IdentityService.Application.UserModule.Dto;
using IdentityService.Application.UserModule.Query.Get;
using IdentityService.Application.UserModule.Query.List;
using Moq;
using Shared.Logging.Contracts;
using Xunit;

namespace IdentityService.UnitTests.UserModule;

public class UserModuleHandlerTests
{
    [Fact]
    public async Task GetUsersHandler_ShouldDelegateToUserService()
    {
        var expected = new UserListResponseDto
        {
            Items = [new UserListItemDto { Id = Guid.NewGuid(), Email = "ada@example.com", DisplayName = "Ada" }]
        };
        var service = new Mock<IUserService>();
        var logger = new Mock<ILoggerManager<GetUsersQueryHandler>>();
        service.Setup(x => x.GetUsers("ada", CancellationToken.None)).ReturnsAsync(expected);
        var handler = new GetUsersQueryHandler(service.Object, logger.Object);

        UserListResponseDto actual = await handler.Handle(new GetUsersQuery("ada"), CancellationToken.None);

        actual.Should().BeSameAs(expected);
    }

    [Fact]
    public void GetUsersValidator_ShouldFail_WhenSearchIsTooLong()
    {
        var result = new GetUsersQueryValidator().Validate(new GetUsersQuery(new string('a', 151)));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage == "Search cannot exceed 150 characters.");
    }

    [Fact]
    public async Task GetUserByIdHandler_ShouldDelegateToUserService()
    {
        Guid userId = Guid.NewGuid();
        var expected = new AuthenticatedUserDto { Id = userId, Email = "ada@example.com", UserName = "Ada" };
        var service = new Mock<IUserService>();
        var logger = new Mock<ILoggerManager<GetUserByIdQueryHandler>>();
        service.Setup(x => x.GetUserById(userId, CancellationToken.None)).ReturnsAsync(expected);
        var handler = new GetUserByIdQueryHandler(service.Object, logger.Object);

        AuthenticatedUserDto actual = await handler.Handle(new GetUserByIdQuery(userId), CancellationToken.None);

        actual.Should().BeSameAs(expected);
    }

    [Fact]
    public void GetUserByIdValidator_ShouldFail_WhenUserIdIsEmpty()
    {
        var result = new GetUserByIdQueryValidator().Validate(new GetUserByIdQuery(Guid.Empty));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage == "UserId cannot be empty.");
    }

    [Fact]
    public async Task AssignUserRoleHandler_ShouldDelegateToUserRoleService()
    {
        Guid userId = Guid.NewGuid();
        Guid roleId = Guid.NewGuid();
        var expected = new UserRoleMappingResponseDto { UserId = userId, RoleId = roleId, MappingId = Guid.NewGuid() };
        var service = new Mock<IUserRoleService>();
        var logger = new Mock<ILoggerManager<AssignUserRoleCommandHandler>>();
        service.Setup(x => x.AssignRoleAsync(userId, roleId, CancellationToken.None)).ReturnsAsync(expected);
        var handler = new AssignUserRoleCommandHandler(service.Object, logger.Object);

        UserRoleMappingResponseDto actual = await handler.Handle(
            new AssignUserRoleCommand(userId, new AssignUserRoleRequestDto { RoleId = roleId }),
            CancellationToken.None);

        actual.Should().BeSameAs(expected);
    }

    [Fact]
    public void AssignUserRoleValidator_ShouldFail_ForMissingUserRequestOrRole()
    {
        var validator = new AssignUserRoleCommandValidator();

        validator.Validate(new AssignUserRoleCommand(Guid.Empty, null!))
            .Errors.Should().Contain(x => x.ErrorMessage == "User ID is required.");
        validator.Validate(new AssignUserRoleCommand(Guid.NewGuid(), null!))
            .Errors.Should().Contain(x => x.ErrorMessage == "Role assignment request is required.");
        validator.Validate(new AssignUserRoleCommand(Guid.NewGuid(), new AssignUserRoleRequestDto()))
            .Errors.Should().Contain(x => x.ErrorMessage == "Role ID is required.");
    }
}
