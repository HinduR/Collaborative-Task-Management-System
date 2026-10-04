using BoardTaskService.Application.BoardModule.Contract.IService;
using BoardTaskService.Application.BoardModule.Dto;
using BoardTaskService.Domain.Models;
using Shared.Common.contracts;
using Shared.Exceptions.Infrastructure;
using Shared.Logging.Contracts;

namespace BoardTaskService.Application.BoardModule.Service;

/// <summary>
/// Orchestrates workflow column updates: validates ownership, membership, and business
/// constraints, then delegates the actual persistence to <see cref="IWorkflowColumnService"/>.
/// </summary>
public class WorkflowColumnHelperService : IWorkflowColumnHelperService
{
    private readonly IWorkflowColumnService _workflowColumnService;
    private readonly IUserContext _userContext;
    private readonly ILoggerManager<WorkflowColumnHelperService> _logger;

    public WorkflowColumnHelperService(
        IWorkflowColumnService workflowColumnService,
        IUserContext userContext,
        ILoggerManager<WorkflowColumnHelperService> logger)
    {
        _workflowColumnService = workflowColumnService;
        _userContext = userContext;
        _logger = logger;
    }

    /// <summary>
    /// Replaces the full set of workflow columns for a board, applying creates, renames,
    /// reorders, and soft-deletes based on the supplied column list. Only the board owner
    /// may perform this action, and columns containing active tasks cannot be removed.
    /// </summary>
    /// <param name="boardId">The unique identifier of the board whose workflow columns are being updated.</param>
    /// <param name="request">The complete desired state of the board's workflow columns.</param>
    /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
    /// <returns>The saved list of workflow columns, reflecting their final names, IDs, and sort order.</returns>
    /// <summary>
    /// Replaces the complete set of workflow columns for a board.
    /// </summary>
    public async Task<List<WorkflowColumnDto>> SaveWorkflowColumnsAsync(
        Guid boardId,
        UpdateWorkflowColumnsRequest request,
        CancellationToken cancellationToken)
    {
        Guid currentUserId = _userContext.GetUserId();

        _logger.LogInformation("Saving workflow columns for board {BoardId} requested by user {UserId}.", boardId, currentUserId);
        Board? board = await _workflowColumnService.GetActiveBoardAsync(boardId, cancellationToken);

        if (board is null)
        {
            _logger.LogError("Workflow column save failed: board {BoardId} not found.", null, boardId);
            throw new NotFoundCustomException("The selected board does not exist.", "Board not found.");
        }

        if (board.CreatedBy != currentUserId)
        {
            _logger.LogError("Workflow column save denied for board {BoardId}: " + "user {UserId} is not the owner.", null, boardId, currentUserId);
            throw new ForBiddenCustomException("Only the Board Owner can update workflow columns.", "Access denied.");
        }

        List<WorkflowColumn> existingColumnList = await _workflowColumnService.GetActiveColumnsByBoardAsync(
                boardId,
                cancellationToken);

        Dictionary<Guid, WorkflowColumn> existingColumnsById = existingColumnList.ToDictionary(column => column.Id);

        HashSet<Guid> suppliedIdSet = request.WorkflowColumnList
            .Where(column => column.Id.HasValue)
            .Select(column => column.Id!.Value)
            .ToHashSet();

        bool containsUnknownColumn = suppliedIdSet.Any(
            id => !existingColumnsById.ContainsKey(id));

        if (containsUnknownColumn)
        {
            _logger.LogError(
                "Workflow column save rejected for board {BoardId}: " +
                "unknown column ID referenced.",
                null,
                boardId);

            throw new BadRequestCustomException("One or more workflow columns do not belong to this board.", "Invalid workflow column.");
        }

        string[] normalizedNames = request.WorkflowColumnList
            .Select(column => column.ColumnName.Trim())
            .ToArray();

        List<WorkflowColumn> removedColumnList = existingColumnList
            .Where(column => !suppliedIdSet.Contains(column.Id))
            .ToList();

        await ValidateRemovedColumns(boardId, removedColumnList, cancellationToken);

        List<WorkflowColumn> changedColumns = [];
        List<WorkflowColumn> newColumns = [];
        List<WorkflowColumnDto> result = [];

        for (int index = 0; index < request.WorkflowColumnList.Count; index++)
        {
            WorkflowColumnRequest requestedColumn =
                request.WorkflowColumnList[index];

            string columnName = normalizedNames[index];
            int sortOrder = index + 1;

            Guid[] normalizedRoleIds = NormalizeRoleIds(
                requestedColumn.RoleIds);

            WorkflowColumn workflowColumn;

            if (requestedColumn.Id is Guid existingColumnId)
            {
                workflowColumn = existingColumnsById[existingColumnId];

                bool nameChanged = workflowColumn.Name != columnName;

                bool sortOrderChanged = workflowColumn.SortOrder != sortOrder;

                bool roleIdsChanged = !HaveSameRoleIds(workflowColumn.RoleId, normalizedRoleIds);

                if (nameChanged ||sortOrderChanged || roleIdsChanged)
                {
                    workflowColumn.Name = columnName;
                    workflowColumn.SortOrder = sortOrder;
                    workflowColumn.RoleId = normalizedRoleIds;

                    changedColumns.Add(workflowColumn);
                }
            }
            else
            {
                workflowColumn = new WorkflowColumn
                {
                    Id = Guid.NewGuid(),
                    BoardId = boardId,
                    Name = columnName,
                    SortOrder = sortOrder,
                    RoleId = normalizedRoleIds,
                    IsActive = true
                };

                newColumns.Add(workflowColumn);
            }

            result.Add(MapToDto(workflowColumn));
        }

        foreach (WorkflowColumn removedColumn in removedColumnList)
        {
            removedColumn.IsActive = false;
        }

        await _workflowColumnService.PersistColumnChangesAsync(
            changedColumns,
            removedColumnList,
            newColumns,
            cancellationToken);

        _logger.LogInformation(
            "Workflow columns saved for board {BoardId} by user {UserId}. " +
            "Created: {CreatedCount}, Updated: {UpdatedCount}, " +
            "Removed: {RemovedCount}.",
            boardId,
            currentUserId,
            newColumns.Count,
            changedColumns.Count,
            removedColumnList.Count);

        return result;
    }

    private async Task ValidateRemovedColumns(
        Guid boardId,
        List<WorkflowColumn> removedColumns,
        CancellationToken cancellationToken)
    {
        if (removedColumns.Count == 0)
        {
            return;
        }

        List<Guid> removedColumnIds = removedColumns
            .Select(column => column.Id)
            .ToList();

        bool hasActiveTasks =
            await _workflowColumnService.HasActiveTasksInColumnsAsync(
                removedColumnIds,
                cancellationToken);

        if (!hasActiveTasks)
        {
            return;
        }

        _logger.LogError(
            "Workflow column save rejected for board {BoardId}: " +
            "attempted to remove a column containing active tasks.",
            null,
            boardId);

        throw new BadRequestCustomException(
            "A workflow column containing tasks cannot be deleted. " +
            "Move or delete its tasks first.",
            "Workflow column contains tasks.");
    }

    private static Guid[] NormalizeRoleIds(
        IEnumerable<Guid>? roleIds)
    {
        return roleIds?
            .Where(roleId => roleId != Guid.Empty)
            .Distinct()
            .OrderBy(roleId => roleId)
            .ToArray() ?? [];
    }

    private static bool HaveSameRoleIds(
        IEnumerable<Guid>? existingRoleIds,
        IEnumerable<Guid>? requestedRoleIds)
    {
        HashSet<Guid> existingIds = (existingRoleIds ?? [])
            .Where(roleId => roleId != Guid.Empty)
            .ToHashSet();

        HashSet<Guid> requestedIds = (requestedRoleIds ?? [])
            .Where(roleId => roleId != Guid.Empty)
            .ToHashSet();

        return existingIds.SetEquals(requestedIds);
    }

    private static WorkflowColumnDto MapToDto(
        WorkflowColumn column)
    {
        return new WorkflowColumnDto
        {
            Id = column.Id,
            ColumnName = column.Name,
            SortOrder = column.SortOrder,
            RoleIds = column.RoleId.ToList()
        };
    }
}