using BoardTaskService.Application.ProjectModule.Contract.IService;
using BoardTaskService.Application.ProjectModule.Dto;
using BoardTaskService.Application.ProjectModule.Query.List;
using FluentAssertions;
using Moq;
using Shared.Logging.Contracts;
using Xunit;

namespace BoardTaskService.UnitTests.ProjectModule.Query.List;

public class GetUserProjectMappingsQueryHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnUserProjectMappings()
    {
        var expected = new List<UserProjectMappingsDto>
        {
            new() { UserId = Guid.NewGuid(), ProjectIdList = [Guid.NewGuid()] }
        };
        var service = new Mock<IProjectService>();
        var logger = new Mock<ILoggerManager<GetUserProjectMappingsQueryHandler>>();

        service.Setup(x => x.GetUserProjectMappings(CancellationToken.None)).ReturnsAsync(expected);

        var handler = new GetUserProjectMappingsQueryHandler(service.Object, logger.Object);

        List<UserProjectMappingsDto> actual = await handler.Handle(
            new GetUserProjectMappingsQuery(),
            CancellationToken.None);

        actual.Should().BeSameAs(expected);
    }
}
