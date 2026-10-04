using BoardTaskService.Application.BoardModule.Dto;
using BoardTaskService.Domain.Models;

namespace BoardTaskService.Application.BoardModule.Contract.IService;

/// <summary>
/// Represents the IWorkflowColumnService component.
/// </summary>
public interface IWorkflowColumnService
{
    /// <summary>
    /// Retrieves the active board with the specified identifier.
    /// </summary>
    /// <param name="boardId">The unique identifier of the board.</param>
    /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
    /// <returns>The active <see cref="Board"/>, or <see langword="null"/> if none was found.</returns>

    Task<Board?> GetActiveBoardAsync(
         Guid boardId,
         CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves all active workflow columns belonging to the specified board.
    /// </summary>
    /// <param name="boardId">The unique identifier of the board.</param>
    /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
    /// <returns>A list of active <see cref="WorkflowColumn"/> entities for the board.</returns>
    Task<List<WorkflowColumn>> GetActiveColumnsByBoardAsync(
        Guid boardId,
        CancellationToken cancellationToken);
    /// <summary>
    /// Determines whether any active board task currently belongs to one of the specified workflow columns.
    /// </summary>
    /// <param name="columnIds">The workflow column identifiers to check.</param>
    /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
    /// <returns><see langword="true"/> if at least one active task references any of the given columns; otherwise, <see langword="false"/>.</returns>
    Task<bool> HasActiveTasksInColumnsAsync(
        IEnumerable<Guid> columnIds,
        CancellationToken cancellationToken);

    /// <summary>
    /// Persists workflow column changes â€” updates, soft-deletes, and creations â€” in a single save operation.
    /// </summary>
    /// <param name="changedColumns">Existing columns whose name or sort order has changed.</param>
    /// <param name="removedColumns">Existing columns that have been soft-deleted (<see cref="WorkflowColumn.IsActive"/> set to <see langword="false"/>).</param>
    /// <param name="newColumns">New workflow columns to be created.</param>
    /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
    Task PersistColumnChangesAsync(
        List<WorkflowColumn> changedColumns,
        List<WorkflowColumn> removedColumns,
        List<WorkflowColumn> newColumns,
        CancellationToken cancellationToken);
}
