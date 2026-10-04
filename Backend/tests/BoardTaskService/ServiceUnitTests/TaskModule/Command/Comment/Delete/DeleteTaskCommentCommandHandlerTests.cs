using BoardTaskService.Application.TaskModule.Command.Comment.Delete;
using BoardTaskService.Application.TaskModule.Contract.IHelperService;
using FluentAssertions;
using Moq;
using Shared.Logging.Contracts;
using Xunit;

namespace BoardTaskService.UnitTests.TaskModule.Command.Comment.Delete;

public class DeleteTaskCommentCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldDeleteComment()
    {
        Guid boardId = Guid.NewGuid();
        Guid taskId = Guid.NewGuid();
        Guid commentId = Guid.NewGuid();
        var service = new Mock<ITaskCommentHelperService>();
        var logger = new Mock<ILoggerManager<DeleteTaskCommentCommandHandler>>();
        var handler = new DeleteTaskCommentCommandHandler(service.Object, logger.Object);

        await handler.Handle(new DeleteTaskCommentCommand(boardId, taskId, commentId), CancellationToken.None);

        service.Verify(x => x.DeleteComment(boardId, taskId, commentId, CancellationToken.None), Times.Once);
    }

    [Fact]
    public void Validator_ShouldFail_ForMissingIds()
    {
        var result = new DeleteTaskCommentCommandValidator().Validate(
            new DeleteTaskCommentCommand(Guid.Empty, Guid.Empty, Guid.Empty));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage == "Board id is required.");
        result.Errors.Should().Contain(x => x.ErrorMessage == "Task id is required.");
        result.Errors.Should().Contain(x => x.ErrorMessage == "Comment id is required.");
    }
}
