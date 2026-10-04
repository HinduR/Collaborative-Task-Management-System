using BoardTaskService.Application.TaskModule.Command.Delete;
using BoardTaskService.Application.TaskModule.Contract.IService;
using FluentAssertions;
using Moq;
using Shared.Logging.Contracts;
using Xunit;

namespace BoardTaskService.UnitTests.TaskModule.Command.Delete;

public class DeleteTaskCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldDeleteTask()
    {
        Guid boardId = Guid.NewGuid();
        Guid taskId = Guid.NewGuid();
        var service = new Mock<ITaskService>();
        var logger = new Mock<ILoggerManager<DeleteTaskCommandHandler>>();
        var handler = new DeleteTaskCommandHandler(service.Object, logger.Object);

        await handler.Handle(new DeleteTaskCommand(boardId, taskId), CancellationToken.None);

        service.Verify(x => x.DeleteTask(boardId, taskId, CancellationToken.None), Times.Once);
    }

    [Fact]
    public void Validator_ShouldFail_ForMissingIds()
    {
        var result = new DeleteTaskCommandValidator().Validate(new DeleteTaskCommand(Guid.Empty, Guid.Empty));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == "BoardId");
        result.Errors.Should().Contain(x => x.PropertyName == "TaskId");
    }
}
