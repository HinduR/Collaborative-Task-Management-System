using BoardTaskService.Domain.Models;

namespace BoardTaskService.Application.TaskModule.Contract.IService;

/// <summary>Defines data-access operations for task comments and their related tasks.</summary>
public interface ICommentService
{
    /// <summary>Determines whether an active task exists in the specified board.</summary>
    /// <param name="boardId">The unique identifier of the board.</param>
    /// <param name="taskId">The unique identifier of the task.</param>
    /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
    /// <returns><see langword="true"/> when the task exists in the board; otherwise, <see langword="false"/>.</returns>
    Task<bool> TaskExistsInBoardAsync(
        Guid boardId,
        Guid taskId,
        CancellationToken cancellationToken);

    /// <summary>Retrieves an active comment belonging to the specified task.</summary>
    /// <param name="taskId">The unique identifier of the task containing the comment.</param>
    /// <param name="commentId">The unique identifier of the comment.</param>
    /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
    /// <returns>The matching active comment, or <see langword="null"/> when no comment is found.</returns>
    Task<TaskComment?> GetActiveCommentAsync(
        Guid taskId,
        Guid commentId,
        CancellationToken cancellationToken);

    /// <summary>Creates a task comment.</summary>
    /// <param name="comment">The task comment to create.</param>
    /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
    Task CreateCommentAsync(
        TaskComment comment,
        CancellationToken cancellationToken);

    /// <summary>Updates an existing task comment.</summary>
    /// <param name="comment">The task comment to update.</param>
    /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
    Task UpdateCommentAsync(
        TaskComment comment,
        CancellationToken cancellationToken);

    /// <summary>Deletes an existing task comment.</summary>
    /// <param name="comment">The task comment to delete.</param>
    /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
    Task DeleteCommentAsync(
        TaskComment comment,
        CancellationToken cancellationToken);
}
