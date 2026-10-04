using FluentAssertions;
using IdentityService.Application.AuthenticationModule.Contract.IService;
using IdentityService.Application.AuthenticationModule.Query.Validate;
using Moq;
using Xunit;

namespace IdentityService.UnitTests.AuthModule.Query;

public class ValidateAccessTokenQueryHandlerTests
{
    [Fact]
    public async Task Handle_ShouldDelegateToAuthenticationService()
    {
        var service = new Mock<IAuthenticationService>();
        service.Setup(x => x.ValidateAccessTokenAsync("token", CancellationToken.None)).ReturnsAsync(true);
        var handler = new ValidateAccessTokenQueryHandler(service.Object);

        bool actual = await handler.Handle(new ValidateAccessTokenQuery("token"), CancellationToken.None);

        actual.Should().BeTrue();
    }

    [Fact]
    public void Validator_ShouldFail_WhenAccessTokenIsMissing()
    {
        var result = new ValidateAccessTokenQueryValidator().Validate(new ValidateAccessTokenQuery(""));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage == "Access token is required.");
    }
}
