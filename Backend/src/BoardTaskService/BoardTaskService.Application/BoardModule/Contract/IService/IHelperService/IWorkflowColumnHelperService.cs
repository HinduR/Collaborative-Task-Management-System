using BoardTaskService.Application.BoardModule.Dto;

namespace BoardTaskService.Application.BoardModule.Contract.IService;

/// <summary>
/// Contains the business logic for validating and applying workflow column changes for a board.
/// </summary>
public interface IWorkflowColumnHelperService
{
    /// <summary>
    /// Replaces the full set of workflow columns for a board, applying creates, renames,
    /// reorders, and soft-deletes based on the supplied column list. Only the board owner
    /// may perform this action, and columns containing active tasks cannot be removed.
    /// </summary>
    /// <param name="boardId">The unique identifier of the board whose workflow columns are being updated.</param>
    /// <param name="request">The complete desired state of the board's workflow columns.</param>
    /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
    /// <returns>The saved list of workflow columns, reflecting their final names, IDs, and sort order.</returns>
    Task<List<WorkflowColumnDto>> SaveWorkflowColumnsAsync(
        Guid boardId,
        UpdateWorkflowColumnsRequest request,
        CancellationToken cancellationToken);
}
