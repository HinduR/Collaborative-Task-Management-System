using FluentAssertions;
using IdentityService.Application.RoleModule.Contract.IService;
using IdentityService.Application.RoleModule.Query.Get;
using IdentityService.Domain.Models;
using Moq;
using Shared.Logging.Contracts;
using Xunit;

namespace IdentityService.UnitTests.RoleModule;

public class GetRolesQueryHandlerTests
{
    [Fact]
    public async Task Handle_ShouldMapActiveRolesToDtos()
    {
        var roles = new List<Role>
        {
            new() { Id = Guid.NewGuid(), Name = "Admin", Description = "Administrator" },
            new() { Id = Guid.NewGuid(), Name = "Member" }
        };
        var service = new Mock<IRoleService>();
        var logger = new Mock<ILoggerManager<GetRolesQueryHandler>>();
        service.Setup(x => x.GetActiveRolesAsync(CancellationToken.None)).ReturnsAsync(roles);
        var handler = new GetRolesQueryHandler(service.Object, logger.Object);

        var result = await handler.Handle(new GetRolesQuery(), CancellationToken.None);

        result.Should().HaveCount(2);
        result[0].Id.Should().Be(roles[0].Id);
        result[0].Name.Should().Be("Admin");
        result[0].Description.Should().Be("Administrator");
        result[1].Name.Should().Be("Member");
    }
}
