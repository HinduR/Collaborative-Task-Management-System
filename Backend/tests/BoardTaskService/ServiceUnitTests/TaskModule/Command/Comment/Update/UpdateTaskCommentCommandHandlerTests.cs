using BoardTaskService.Application.TaskModule.Command.Comment.Update;
using BoardTaskService.Application.TaskModule.Contract.IHelperService;
using BoardTaskService.Application.TaskModule.Dto;
using FluentAssertions;
using Moq;
using Shared.Logging.Contracts;
using Xunit;

namespace BoardTaskService.UnitTests.TaskModule.Command.Comment.Update;

public class UpdateTaskCommentCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldUpdateCommentAndReturnDto()
    {
        Guid boardId = Guid.NewGuid();
        Guid taskId = Guid.NewGuid();
        Guid commentId = Guid.NewGuid();
        var request = new CommentRequestDto { Comment = "Updated" };
        var expected = new TaskCommentDto { Id = commentId, Comment = request.Comment };
        var service = new Mock<ITaskCommentHelperService>();
        var logger = new Mock<ILoggerManager<UpdateTaskCommentCommandHandler>>();

        service.Setup(x => x.UpdateComment(boardId, taskId, commentId, request, CancellationToken.None)).ReturnsAsync(expected);

        var handler = new UpdateTaskCommentCommandHandler(service.Object, logger.Object);

        TaskCommentDto actual = await handler.Handle(
            new UpdateTaskCommentCommand(boardId, taskId, commentId, request),
            CancellationToken.None);

        actual.Should().BeSameAs(expected);
    }

    [Fact]
    public void Validator_ShouldFail_ForMissingValues()
    {
        var result = new UpdateTaskCommentCommandValidator().Validate(
            new UpdateTaskCommentCommand(Guid.Empty, Guid.Empty, Guid.Empty, null!));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage == "Board id is required.");
        result.Errors.Should().Contain(x => x.ErrorMessage == "Task id is required.");
        result.Errors.Should().Contain(x => x.ErrorMessage == "Comment id is required.");
        result.Errors.Should().Contain(x => x.ErrorMessage == "Comment request is required.");
    }
}
