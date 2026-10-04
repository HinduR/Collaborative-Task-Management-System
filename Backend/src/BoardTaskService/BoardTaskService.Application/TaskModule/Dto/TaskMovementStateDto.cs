namespace BoardTaskService.Application.TaskModule.Dto;

/// <summary>
/// Contains the persisted state required to move a task.
/// </summary>
public sealed record TaskMovementStateDto(
    Guid TaskId,
    Guid WorkflowColumnId);
