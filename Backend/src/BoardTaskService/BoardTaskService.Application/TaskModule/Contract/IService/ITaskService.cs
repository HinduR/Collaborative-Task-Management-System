using BoardTaskService.Application.TaskModule.Dto;

namespace BoardTaskService.Application.TaskModule.Contract.IService;

/// <summary>Defines operations for creating, retrieving, updating, and deleting tasks.</summary>
public interface ITaskService
{
    /// <summary>Creates a task in the specified board.</summary>
    /// <param name="boardId">The unique identifier of the board in which the task will be created.</param>
    /// <param name="request">The task creation data.</param>
    /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
    /// <returns>Summary information about the newly created task.</returns>
    Task<TaskSummaryDto> CreateTask(
        Guid boardId,
        TaskRequestDto request,
        CancellationToken cancellationToken);

    /// <summary>Retrieves a task by its identifier from the specified board.</summary>
    /// <param name="boardId">The unique identifier of the board containing the task.</param>
    /// <param name="taskId">The unique identifier of the task to retrieve.</param>
    /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
    /// <returns>The details of the requested task.</returns>
    Task<TaskDetailsDto> GetTaskById(
        Guid boardId,
        Guid taskId,
        CancellationToken cancellationToken);

    /// <summary>Updates an existing task in the specified board.</summary>
    /// <param name="boardId">The unique identifier of the board containing the task.</param>
    /// <param name="taskId">The unique identifier of the task to update.</param>
    /// <param name="request">The updated task data.</param>
    /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
    /// <returns>The updated task details.</returns>
    Task<TaskDetailsDto> UpdateTask(
        Guid boardId,
        Guid taskId,
        TaskRequestDto request,
        CancellationToken cancellationToken);

    /// <summary>Deletes the specified task from a board.</summary>
    /// <param name="boardId">The unique identifier of the board containing the task.</param>
    /// <param name="taskId">The unique identifier of the task to delete.</param>
    /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
    Task DeleteTask(
        Guid boardId,
        Guid taskId,
        CancellationToken cancellationToken);
}
