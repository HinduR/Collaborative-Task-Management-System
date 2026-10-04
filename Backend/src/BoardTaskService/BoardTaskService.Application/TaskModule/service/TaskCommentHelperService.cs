using BoardTaskService.Application.TaskModule.Contract.IHelperService;
using BoardTaskService.Application.TaskModule.Contract.IService;
using BoardTaskService.Application.TaskModule.Dto;
using BoardTaskService.Domain.Models;
using Microsoft.AspNetCore.Http;
using Shared.Common.contracts;
using Shared.Exceptions.Infrastructure;
using Shared.Logging.Contracts;

namespace BoardTaskService.Application.TaskModule.HelperService;

/// <summary>Handles task comment business rules, ownership validation, and comment operations.</summary>
public class TaskCommentHelperService : ITaskCommentHelperService
{
    private readonly ICommentService _commentService;
    private readonly IUserContext _userContext;
    private readonly ILoggerManager<TaskCommentHelperService> _logger;

    /// <summary>Initializes a new instance of the <see cref="TaskCommentHelperService"/> class.</summary>
    /// <param name="commentService">Service used to access and persist task comments.</param>
    /// <param name="userContext">Provides information about the current authenticated user.</param>
    /// <param name="logger">Logger used to record task comment activity.</param>
    public TaskCommentHelperService(
        ICommentService commentService,
        IUserContext userContext,
        ILoggerManager<TaskCommentHelperService> logger)
    {
        _commentService = commentService;
        _userContext = userContext;
        _logger = logger;
    }

    /// <summary>Creates a comment for the specified task.</summary>
    /// <param name="boardId">The unique identifier of the board containing the task.</param>
    /// <param name="taskId">The unique identifier of the task receiving the comment.</param>
    /// <param name="request">The comment data to create.</param>
    /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
    /// <returns>The newly created task comment.</returns>
    public async Task<TaskCommentDto> CreateComment(
        Guid boardId,
        Guid taskId,
        CommentRequestDto request,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug(
            "Executing CreateComment.");

        _logger.LogInformation("Creating a comment for task {TaskId} in board {BoardId}.", taskId, boardId);

        await EnsureTaskExists(boardId, taskId, cancellationToken);

        TaskComment comment = new()
        {
            TaskId = taskId,
            Comment = request.Comment.Trim()
        };

        await _commentService.CreateCommentAsync(comment, cancellationToken);

        _logger.LogInformation("Comment {CommentId} created for task {TaskId}.", comment.Id, taskId);

        return MapToDto(comment, _userContext.GetUserName(), true);
    }

    /// <summary>Updates an existing comment after validating task existence and comment ownership.</summary>
    /// <param name="boardId">The unique identifier of the board containing the task.</param>
    /// <param name="taskId">The unique identifier of the task containing the comment.</param>
    /// <param name="commentId">The unique identifier of the comment to update.</param>
    /// <param name="request">The updated comment data.</param>
    /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
    /// <returns>The updated task comment.</returns>
    public async Task<TaskCommentDto> UpdateComment(
        Guid boardId,
        Guid taskId,
        Guid commentId,
        CommentRequestDto request,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug(
            "Executing UpdateComment.");

        _logger.LogInformation("Updating comment {CommentId} for task {TaskId} in board {BoardId}.", commentId, taskId, boardId);

        await EnsureTaskExists(boardId, taskId, cancellationToken);

        TaskComment comment = await GetComment(taskId, commentId, cancellationToken);

        EnsureCommentOwner(comment, "update");

        comment.Comment = request.Comment.Trim();

        await _commentService.UpdateCommentAsync(comment, cancellationToken);

        _logger.LogInformation("Comment {CommentId} updated for task {TaskId}.", commentId, taskId);

        return MapToDto(comment, _userContext.GetUserName(), true);
    }

    /// <summary>Deletes an existing comment after validating task existence and comment ownership.</summary>
    /// <param name="boardId">The unique identifier of the board containing the task.</param>
    /// <param name="taskId">The unique identifier of the task containing the comment.</param>
    /// <param name="commentId">The unique identifier of the comment to delete.</param>
    /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
    public async Task DeleteComment(
        Guid boardId,
        Guid taskId,
        Guid commentId,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug(
            "Executing DeleteComment.");

        _logger.LogInformation("Deleting comment {CommentId} from task {TaskId} in board {BoardId}.", commentId, taskId, boardId);

        await EnsureTaskExists(boardId, taskId, cancellationToken);

        TaskComment comment = await GetComment(taskId, commentId, cancellationToken);

        EnsureCommentOwner(comment, "delete");

        await _commentService.DeleteCommentAsync(comment, cancellationToken);

        _logger.LogInformation("Comment {CommentId} deleted from task {TaskId}.", commentId, taskId);
    }

    /// <summary>Ensures that the specified active task exists within the requested board.</summary>
    /// <param name="boardId">The unique identifier of the board.</param>
    /// <param name="taskId">The unique identifier of the task.</param>
    /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
    private async Task EnsureTaskExists(
        Guid boardId,
        Guid taskId,
        CancellationToken cancellationToken)
    {
        bool taskExists = await _commentService.TaskExistsInBoardAsync(boardId, taskId, cancellationToken);

        if (!taskExists)
        {
            _logger.LogError("Task comment operation failed because task {TaskId} was not found in board {BoardId}.", null, taskId, boardId);

            throw new NotFoundCustomException(
                "Task not found.",
                "The task was not found in the specified board.");
        }
    }

    /// <summary>Retrieves the specified active comment belonging to a task.</summary>
    /// <param name="taskId">The unique identifier of the task.</param>
    /// <param name="commentId">The unique identifier of the comment.</param>
    /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
    /// <returns>The active task comment.</returns>
    private async Task<TaskComment> GetComment(
        Guid taskId,
        Guid commentId,
        CancellationToken cancellationToken)
    {
        TaskComment? comment = await _commentService.GetActiveCommentAsync(taskId, commentId, cancellationToken);

        if (comment is null)
        {
            _logger.LogError("Task comment operation failed because comment {CommentId} was not found for task {TaskId}.", null, commentId, taskId);

            throw new NotFoundCustomException(
                "Comment not found.",
                "The specified comment was not found.");
        }

        return comment;
    }

    /// <summary>Ensures that the current authenticated user owns the specified comment.</summary>
    /// <param name="comment">The comment whose ownership is being validated.</param>
    /// <param name="action">The action the current user is attempting to perform.</param>
    private void EnsureCommentOwner(
        TaskComment comment,
        string action)
    {
        Guid currentUserId = _userContext.GetUserId();

        if (comment.CreatedBy != currentUserId)
        {
            _logger.LogError("Comment {CommentId} access denied for user {UserId} while attempting to {Action} the comment.", null, comment.Id, currentUserId, action);

            throw new ForBiddenCustomException(
                "Comment access denied.",
                string.Concat(
                    "You can ",
                    action,
                    " only comments created by you."));
        }
    }

    /// <summary>Maps a task comment entity to its response DTO.</summary>
    /// <param name="comment">The task comment entity to map.</param>
    /// <param name="createdByName">The display name of the user who created the comment.</param>
    /// <param name="isCommentOwner">Indicates whether the current user owns the comment.</param>
    /// <returns>The mapped task comment DTO.</returns>
    private static TaskCommentDto MapToDto(
        TaskComment comment,
        string createdByName,
        bool isCommentOwner)
    {
        return new TaskCommentDto
        {
            Id = comment.Id,
            Comment = comment.Comment,
            CreatedByName = createdByName,
            IsCommentOwner = isCommentOwner
        };
    }
}
