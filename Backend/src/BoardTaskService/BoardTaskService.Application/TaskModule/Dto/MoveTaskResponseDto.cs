namespace BoardTaskService.Application.TaskModule.Dto;

/// <summary>
/// Represents the MoveTaskResponseDto component.
/// </summary>
public class MoveTaskResponseDto
{
    /// <summary>
    /// Gets or sets the TaskId value.
    /// </summary>
    public Guid TaskId { get; set; }

    /// <summary>
    /// Gets or sets the WorkflowColumnId value.
    /// </summary>
    public Guid WorkflowColumnId { get; set; }
}
