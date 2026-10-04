using BoardTaskService.Application.ProjectModule.Contract.IService;
using BoardTaskService.Application.ProjectModule.Dto;
using BoardTaskService.Application.ProjectModule.Query.List;
using FluentAssertions;
using Moq;
using Shared.Logging.Contracts;
using Xunit;

namespace BoardTaskService.UnitTests.ProjectModule.Query.List;

public class GetProjectQueryHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnProjects()
    {
        var expected = new List<ProjectSummaryDto>
        {
            new() { Id = Guid.NewGuid(), ProjectName = "Round Table", IsAssigned = true }
        };
        var service = new Mock<IProjectService>();
        var logger = new Mock<ILoggerManager<GetProjectQueryHandler>>();

        service.Setup(x => x.GetProjects(CancellationToken.None)).ReturnsAsync(expected);

        var handler = new GetProjectQueryHandler(service.Object, logger.Object);

        List<ProjectSummaryDto> actual = await handler.Handle(new GetProjectQuery(), CancellationToken.None);

        actual.Should().BeSameAs(expected);
    }
}
