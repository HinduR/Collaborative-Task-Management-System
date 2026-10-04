using BoardTaskService.Application.TaskModule.Command.Move;
using BoardTaskService.Application.TaskModule.Contract.IService;
using BoardTaskService.Application.TaskModule.Dto;
using FluentAssertions;
using Moq;
using Shared.Logging.Contracts;
using Xunit;

namespace BoardTaskService.UnitTests.TaskModule.Command.Move;

public class MoveTaskCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldMoveTaskAndReturnResponse()
    {
        Guid boardId = Guid.NewGuid();
        Guid taskId = Guid.NewGuid();
        Guid workflowColumnId = Guid.NewGuid();
        var expected = new MoveTaskResponseDto { TaskId = taskId, WorkflowColumnId = workflowColumnId };
        var service = new Mock<ITaskMovementService>();
        var logger = new Mock<ILoggerManager<MoveTaskCommandHandler>>();

        service.Setup(x => x.MoveTask(boardId, taskId, workflowColumnId, CancellationToken.None)).ReturnsAsync(expected);

        var handler = new MoveTaskCommandHandler(service.Object, logger.Object);

        MoveTaskResponseDto actual = await handler.Handle(
            new MoveTaskCommand(boardId, taskId, new MoveTaskRequestDto { WorkflowColumnId = workflowColumnId }),
            CancellationToken.None);

        actual.Should().BeSameAs(expected);
    }

    [Fact]
    public void Validator_ShouldFail_ForMissingIdsAndRequest()
    {
        var result = new MoveTaskCommandValidator().Validate(
            new MoveTaskCommand(Guid.Empty, Guid.Empty, null!));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage == "The board identifier is required.");
        result.Errors.Should().Contain(x => x.ErrorMessage == "The task identifier is required.");
        result.Errors.Should().Contain(x => x.ErrorMessage == "The task movement request is required.");
    }

    [Fact]
    public void Validator_ShouldFail_ForMissingWorkflowColumnId()
    {
        var result = new MoveTaskCommandValidator().Validate(
            new MoveTaskCommand(Guid.NewGuid(), Guid.NewGuid(), new MoveTaskRequestDto()));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage == "The destination workflow column identifier is required.");
    }
}
