using BoardTaskService.Domain.Models;

namespace BoardTaskService.Application.TaskModule.Contract.IService;

/// <summary>Defines data-access operations required to validate and persist task movements.</summary>
public interface ITaskMovementDataService
{
    /// <summary>Determines whether the specified user has active access to a board.</summary>
    /// <param name="boardId">The unique identifier of the board.</param>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
    /// <returns><see langword="true"/> when the user has board access; otherwise, <see langword="false"/>.</returns>
    Task<bool> HasBoardAccess(
        Guid boardId,
        Guid userId,
        CancellationToken cancellationToken);

    /// <summary>Retrieves an active task from the specified board.</summary>
    /// <param name="boardId">The unique identifier of the board containing the task.</param>
    /// <param name="taskId">The unique identifier of the task to retrieve.</param>
    /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
    /// <returns>The matching task, or <see langword="null"/> when no task is found.</returns>
    Task<BoardTask?> GetTask(
        Guid boardId,
        Guid taskId,
        CancellationToken cancellationToken);

    /// <summary>Determines whether the destination workflow column exists in the specified board.</summary>
    /// <param name="boardId">The unique identifier of the board.</param>
    /// <param name="workflowColumnId">The unique identifier of the workflow column.</param>
    /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
    /// <returns><see langword="true"/> when the workflow column exists; otherwise, <see langword="false"/>.</returns>
    Task<bool> WorkflowColumnExists(
        Guid boardId,
        Guid workflowColumnId,
        CancellationToken cancellationToken);

    /// <summary>Persists changes made to a task.</summary>
    /// <param name="task">The task entity to save.</param>
    /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
    Task SaveTask(
        BoardTask task,
        CancellationToken cancellationToken);
}
