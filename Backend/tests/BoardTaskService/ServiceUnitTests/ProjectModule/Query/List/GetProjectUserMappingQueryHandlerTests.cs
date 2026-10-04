using BoardTaskService.Application.ProjectModule.Contract.IService;
using BoardTaskService.Application.ProjectModule.Dto;
using BoardTaskService.Application.ProjectModule.Query.List;
using FluentAssertions;
using Moq;
using Shared.Logging.Contracts;
using Xunit;

namespace BoardTaskService.UnitTests.ProjectModule.Query.List;

public class GetProjectUserMappingQueryHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnProjectUserMappings()
    {
        Guid projectId = Guid.NewGuid();
        var expected = new List<ProjectUserMappingDto> { new() { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), UserName = "Ada" } };
        var service = new Mock<IProjectService>();
        var logger = new Mock<ILoggerManager<GetProjectUserMappingQueryHandler>>();

        service.Setup(x => x.GetProjectUserMappings(projectId, CancellationToken.None)).ReturnsAsync(expected);

        var handler = new GetProjectUserMappingQueryHandler(service.Object, logger.Object);

        List<ProjectUserMappingDto> actual = await handler.Handle(
            new GetProjectUserMappingQuery(projectId),
            CancellationToken.None);

        actual.Should().BeSameAs(expected);
    }

    [Fact]
    public void Validator_ShouldFail_WhenProjectIdIsEmpty()
    {
        var result = new GetProjectUserMappingQueryValidator().Validate(new GetProjectUserMappingQuery(Guid.Empty));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage == "ProjectId cannot be empty.");
    }
}
