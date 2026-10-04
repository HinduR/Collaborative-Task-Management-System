using BoardTaskService.Application.TaskModule.Dto;

namespace BoardTaskService.Application.TaskModule.Contract.IService;

/// <summary>Defines operations for retrieving tasks belonging to a board.</summary>
public interface IBoardTaskListService
{
    /// <summary>Retrieves the active tasks belonging to the specified board.</summary>
    /// <param name="boardId">The unique identifier of the board whose tasks are being retrieved.</param>
    /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
    /// <returns>A list containing the board's task summaries.</returns>
    Task<List<TaskSummaryDto>> GetBoardTasksAsync(
        Guid boardId,
        CancellationToken cancellationToken);
}
