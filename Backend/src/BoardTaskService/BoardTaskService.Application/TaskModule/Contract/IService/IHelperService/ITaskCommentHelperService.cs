using BoardTaskService.Application.TaskModule.Dto;

namespace BoardTaskService.Application.TaskModule.Contract.IHelperService;

/// <summary>Defines business operations for creating, updating, and deleting task comments.</summary>
public interface ITaskCommentHelperService
{
    /// <summary>Creates a comment for the specified task.</summary>
    /// <param name="boardId">The unique identifier of the board containing the task.</param>
    /// <param name="taskId">The unique identifier of the task receiving the comment.</param>
    /// <param name="request">The comment data to create.</param>
    /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
    /// <returns>The newly created task comment.</returns>
    Task<TaskCommentDto> CreateComment(
        Guid boardId,
        Guid taskId,
        CommentRequestDto request,
        CancellationToken cancellationToken);

    /// <summary>Updates an existing comment on the specified task.</summary>
    /// <param name="boardId">The unique identifier of the board containing the task.</param>
    /// <param name="taskId">The unique identifier of the task containing the comment.</param>
    /// <param name="commentId">The unique identifier of the comment to update.</param>
    /// <param name="request">The updated comment data.</param>
    /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
    /// <returns>The updated task comment.</returns>
    Task<TaskCommentDto> UpdateComment(
        Guid boardId,
        Guid taskId,
        Guid commentId,
        CommentRequestDto request,
        CancellationToken cancellationToken);

    /// <summary>Deletes an existing comment from the specified task.</summary>
    /// <param name="boardId">The unique identifier of the board containing the task.</param>
    /// <param name="taskId">The unique identifier of the task containing the comment.</param>
    /// <param name="commentId">The unique identifier of the comment to delete.</param>
    /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
    Task DeleteComment(
        Guid boardId,
        Guid taskId,
        Guid commentId,
        CancellationToken cancellationToken);
}
