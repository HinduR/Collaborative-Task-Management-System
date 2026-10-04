using BoardTaskService.Application.TaskModule.Contract.IService;
using BoardTaskService.Application.TaskModule.Dto;
using BoardTaskService.Application.TaskModule.Query.Get;
using FluentAssertions;
using Moq;
using Shared.Logging.Contracts;
using Xunit;

namespace BoardTaskService.UnitTests.TaskModule.Query.Get;

public class GetTaskByIdQueryHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnTaskDetails()
    {
        Guid boardId = Guid.NewGuid();
        Guid taskId = Guid.NewGuid();
        var expected = new TaskDetailsDto { Id = taskId, Title = "Task" };
        var service = new Mock<ITaskService>();
        var logger = new Mock<ILoggerManager<GetTaskByIdQueryHandler>>();

        service.Setup(x => x.GetTaskById(boardId, taskId, CancellationToken.None)).ReturnsAsync(expected);

        var handler = new GetTaskByIdQueryHandler(service.Object, logger.Object);

        TaskDetailsDto actual = await handler.Handle(new GetTaskByIdQuery(boardId, taskId), CancellationToken.None);

        actual.Should().BeSameAs(expected);
    }

    [Fact]
    public void Validator_ShouldFail_ForMissingIds()
    {
        var result = new GetTaskByIdQueryValidator().Validate(new GetTaskByIdQuery(Guid.Empty, Guid.Empty));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == "BoardId");
        result.Errors.Should().Contain(x => x.PropertyName == "TaskId");
    }
}
