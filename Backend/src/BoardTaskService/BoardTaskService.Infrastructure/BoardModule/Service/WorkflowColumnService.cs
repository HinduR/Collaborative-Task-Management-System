using BoardTaskService.Application.BoardModule.Contract.IService;
using BoardTaskService.Infrastructure.Common;
using BoardTaskService.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Shared.Logging.Contracts;

namespace BoardTaskService.Infrastructure.BoardModule.Service;

/// <summary>
/// Handles generic data access for workflow columns. Contains no business rules â€”
/// all validation and orchestration live in the Application layer's helper service.
/// </summary>
public class WorkflowColumnService : IWorkflowColumnService
{
    private readonly IRepoWrapper _repoWrapper;
    private readonly ILoggerManager<WorkflowColumnService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="WorkflowColumnService"/> class.
    /// </summary>
    /// <param name="repoWrapper">Provides access to the underlying repositories.</param>
    /// <param name="logger">Logger used to record data access activity.</param>
    public WorkflowColumnService(IRepoWrapper repoWrapper, ILoggerManager<WorkflowColumnService> logger)
    {
        _repoWrapper = repoWrapper;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves the active board with the specified identifier.
    /// </summary>
    /// <param name="boardId">The unique identifier of the board.</param>
    /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
    /// <returns>The active <see cref="Board"/>, or <see langword="null"/> if none was found.</returns>
    public async Task<Board?> GetActiveBoardAsync(
         Guid boardId,
         CancellationToken cancellationToken)
    {
        _logger.LogDebug("Fetching active board {BoardId}.", boardId);

        Board? board = await _repoWrapper.BoardRepository
            .FindFirstByConditionAsync(board => board.IsActive &&
                 board.Id == boardId,
                cancellationToken);

        _logger.LogDebug("Active board fetch for {BoardId} returned {Result}.", boardId, board is null ? "no result" : "a result");

        return board;
    }

    /// <summary>
    /// Retrieves all active workflow columns belonging to the specified board.
    /// </summary>
    /// <param name="boardId">The unique identifier of the board.</param>
    /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
    /// <returns>A list of active <see cref="WorkflowColumn"/> entities for the board.</returns>
    public async Task<List<WorkflowColumn>> GetActiveColumnsByBoardAsync(
         Guid boardId,
         CancellationToken cancellationToken)
    {
        _logger.LogDebug(
            "Fetching active workflow columns for board {BoardId}.",
            boardId);

        List<WorkflowColumn> columns = await _repoWrapper.WorkflowColumnRepository
            .FindByCondition(column => column.IsActive &&
                column.BoardId == boardId)
            .ToListAsync(cancellationToken);

        _logger.LogDebug(
            "Fetched {Count} active workflow columns for board {BoardId}.",
            columns.Count,
            boardId);

        return columns;
    }


    /// <summary>
    /// Determines whether any active board task currently belongs to one of the specified workflow columns.
    /// </summary>
    /// <param name="columnIds">The workflow column identifiers to check.</param>
    /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
    /// <returns><see langword="true"/> if at least one active task references any of the given columns; otherwise, <see langword="false"/>.</returns>
    public async Task<bool> HasActiveTasksInColumnsAsync(
        IEnumerable<Guid> columnIds,
        CancellationToken cancellationToken)
    {
        HashSet<Guid> columnIdSet = columnIds.ToHashSet();

        _logger.LogDebug(
            "Checking for active tasks across {ColumnCount} workflow columns.",
            columnIdSet.Count);

        bool hasActiveTasks = await _repoWrapper.BoardTaskRepository
            .AnyByConditionAsync(
                task => task.IsActive && columnIdSet.Contains(task.WorkflowColumnId),
                cancellationToken);

        _logger.LogDebug(
            "Active task check across {ColumnCount} workflow columns returned {Result}.",
            columnIdSet.Count,
            hasActiveTasks);

        return hasActiveTasks;
    }

    /// <summary>
    /// Persists workflow column changes â€” updates, soft-deletes, and creations â€” in a single save operation.
    /// </summary>
    /// <param name="changedColumns">Existing columns whose name or sort order has changed.</param>
    /// <param name="removedColumns">Existing columns that have been soft-deleted (<see cref="WorkflowColumn.IsActive"/> set to <see langword="false"/>).</param>
    /// <param name="newColumns">New workflow columns to be created.</param>
    /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
    public async Task PersistColumnChangesAsync(
        List<WorkflowColumn> changedColumns,
        List<WorkflowColumn> removedColumns,
        List<WorkflowColumn> newColumns,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug(
            "Persisting workflow column changes: {ChangedCount} updated, {RemovedCount} removed, {NewCount} new.",
            changedColumns.Count,
            removedColumns.Count,
            newColumns.Count);

        if (changedColumns.Count > 0)
        {
            _repoWrapper.WorkflowColumnRepository.UpdateRange(changedColumns);
        }

        if (removedColumns.Count > 0)
        {
            _repoWrapper.WorkflowColumnRepository.UpdateRange(removedColumns);
        }

        if (newColumns.Count > 0)
        {
            await _repoWrapper.WorkflowColumnRepository.CreateRangeAsync(
                newColumns,
                cancellationToken);
        }

        await _repoWrapper.SaveChangesAsync(cancellationToken);

        _logger.LogDebug(
            "Workflow column changes persisted successfully.");
    }
}
