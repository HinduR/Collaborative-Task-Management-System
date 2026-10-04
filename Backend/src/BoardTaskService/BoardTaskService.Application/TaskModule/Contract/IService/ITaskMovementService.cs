using BoardTaskService.Application.TaskModule.Dto;

namespace BoardTaskService.Application.TaskModule.Contract.IService;

/// <summary>Defines business operations for moving tasks between workflow columns.</summary>
public interface ITaskMovementService
{
    /// <summary>Moves a task to the specified workflow column.</summary>
    /// <param name="boardId">The unique identifier of the board containing the task.</param>
    /// <param name="taskId">The unique identifier of the task to move.</param>
    /// <param name="workflowColumnId">The unique identifier of the destination workflow column.</param>
    /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
    /// <returns>The result of the task movement operation.</returns>
    Task<MoveTaskResponseDto> MoveTask(
        Guid boardId,
        Guid taskId,
        Guid workflowColumnId,
        CancellationToken cancellationToken);
}
