using BoardTaskService.Application.TaskModule.Contract.IService;
using BoardTaskService.Application.TaskModule.Dto;
using BoardTaskService.Application.TaskModule.HelperService;
using BoardTaskService.Domain.Models;
using FluentAssertions;
using Moq;
using Shared.Common.contracts;
using Shared.Exceptions.Infrastructure;
using Shared.Logging.Contracts;
using Xunit;

namespace BoardTaskService.UnitTests.TaskModule.Service;

public class TaskCommentHelperServiceTests
{
    [Fact]
    public async Task CreateComment_ShouldCreateTrimmedCommentAndReturnOwnerDto()
    {
        Guid userId = Guid.NewGuid();
        Guid boardId = Guid.NewGuid();
        Guid taskId = Guid.NewGuid();
        var commentService = new Mock<ICommentService>();
        var userContext = UserContext(userId, "Grace");
        var service = CreateService(commentService, userContext);

        commentService.Setup(x => x.TaskExistsInBoardAsync(boardId, taskId, CancellationToken.None)).ReturnsAsync(true);

        TaskCommentDto result = await service.CreateComment(
            boardId,
            taskId,
            new CommentRequestDto { Comment = "  hello  " },
            CancellationToken.None);

        result.Comment.Should().Be("hello");
        result.CreatedByName.Should().Be("Grace");
        result.IsCommentOwner.Should().BeTrue();
        commentService.Verify(x => x.CreateCommentAsync(
            It.Is<TaskComment>(comment => comment.TaskId == taskId && comment.Comment == "hello"),
            CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task UpdateComment_ShouldUpdateOwnedComment()
    {
        Guid userId = Guid.NewGuid();
        Guid boardId = Guid.NewGuid();
        Guid taskId = Guid.NewGuid();
        Guid commentId = Guid.NewGuid();
        var existing = new TaskComment { Id = commentId, TaskId = taskId, Comment = "old", CreatedBy = userId };
        var commentService = new Mock<ICommentService>();
        var service = CreateService(commentService, UserContext(userId, "Grace"));

        commentService.Setup(x => x.TaskExistsInBoardAsync(boardId, taskId, CancellationToken.None)).ReturnsAsync(true);
        commentService.Setup(x => x.GetActiveCommentAsync(taskId, commentId, CancellationToken.None)).ReturnsAsync(existing);

        TaskCommentDto result = await service.UpdateComment(
            boardId,
            taskId,
            commentId,
            new CommentRequestDto { Comment = "  updated  " },
            CancellationToken.None);

        result.Comment.Should().Be("updated");
        existing.Comment.Should().Be("updated");
        commentService.Verify(x => x.UpdateCommentAsync(existing, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task DeleteComment_ShouldDeleteOwnedComment()
    {
        Guid userId = Guid.NewGuid();
        Guid boardId = Guid.NewGuid();
        Guid taskId = Guid.NewGuid();
        Guid commentId = Guid.NewGuid();
        var existing = new TaskComment { Id = commentId, TaskId = taskId, Comment = "old", CreatedBy = userId };
        var commentService = new Mock<ICommentService>();
        var service = CreateService(commentService, UserContext(userId, "Grace"));

        commentService.Setup(x => x.TaskExistsInBoardAsync(boardId, taskId, CancellationToken.None)).ReturnsAsync(true);
        commentService.Setup(x => x.GetActiveCommentAsync(taskId, commentId, CancellationToken.None)).ReturnsAsync(existing);

        await service.DeleteComment(boardId, taskId, commentId, CancellationToken.None);

        commentService.Verify(x => x.DeleteCommentAsync(existing, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task CommentOperations_ShouldThrowNotFound_WhenTaskOrCommentDoesNotExist()
    {
        var commentService = new Mock<ICommentService>();
        var service = CreateService(commentService, UserContext(Guid.NewGuid(), "Grace"));

        await service.Invoking(x => x.CreateComment(Guid.NewGuid(), Guid.NewGuid(), new CommentRequestDto { Comment = "x" }, CancellationToken.None))
            .Should().ThrowAsync<NotFoundCustomException>();

        Guid boardId = Guid.NewGuid();
        Guid taskId = Guid.NewGuid();
        Guid commentId = Guid.NewGuid();
        commentService.Setup(x => x.TaskExistsInBoardAsync(boardId, taskId, CancellationToken.None)).ReturnsAsync(true);

        await service.Invoking(x => x.UpdateComment(boardId, taskId, commentId, new CommentRequestDto { Comment = "x" }, CancellationToken.None))
            .Should().ThrowAsync<NotFoundCustomException>();
    }

    [Fact]
    public async Task UpdateComment_ShouldThrowForbidden_WhenCurrentUserDoesNotOwnComment()
    {
        Guid boardId = Guid.NewGuid();
        Guid taskId = Guid.NewGuid();
        Guid commentId = Guid.NewGuid();
        var commentService = new Mock<ICommentService>();
        var service = CreateService(commentService, UserContext(Guid.NewGuid(), "Grace"));

        commentService.Setup(x => x.TaskExistsInBoardAsync(boardId, taskId, CancellationToken.None)).ReturnsAsync(true);
        commentService.Setup(x => x.GetActiveCommentAsync(taskId, commentId, CancellationToken.None))
            .ReturnsAsync(new TaskComment { Id = commentId, TaskId = taskId, Comment = "old", CreatedBy = Guid.NewGuid() });

        await service.Invoking(x => x.UpdateComment(boardId, taskId, commentId, new CommentRequestDto { Comment = "x" }, CancellationToken.None))
            .Should().ThrowAsync<ForBiddenCustomException>();
    }

    private static TaskCommentHelperService CreateService(
        Mock<ICommentService> commentService,
        Mock<IUserContext> userContext)
    {
        return new TaskCommentHelperService(
            commentService.Object,
            userContext.Object,
            Mock.Of<ILoggerManager<TaskCommentHelperService>>());
    }

    private static Mock<IUserContext> UserContext(Guid userId, string userName)
    {
        var userContext = new Mock<IUserContext>();
        userContext.Setup(x => x.GetUserId()).Returns(userId);
        userContext.Setup(x => x.GetUserName()).Returns(userName);
        return userContext;
    }
}
