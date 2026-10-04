namespace BoardTaskService.Application.TaskModule.Dto;

/// <summary>
/// Represents the TaskMovedEventDto component.
/// </summary>
public class TaskMovedEventDto
{
    /// <summary>
    /// Gets or sets the BoardId value.
    /// </summary>
    public Guid BoardId { get; set; }

    /// <summary>
    /// Gets or sets the TaskId value.
    /// </summary>
    public Guid TaskId { get; set; }

    /// <summary>
    /// Gets or sets the PreviousWorkflowColumnId value.
    /// </summary>
    public Guid PreviousWorkflowColumnId { get; set; }

    /// <summary>
    /// Gets or sets the WorkflowColumnId value.
    /// </summary>
    public Guid WorkflowColumnId { get; set; }

    /// <summary>
    /// Gets or sets the MovedByUserId value.
    /// </summary>
    public Guid MovedByUserId { get; set; }

    /// <summary>
    /// Gets or sets the MovedAt value.
    /// </summary>
    public DateTime MovedAt { get; set; }
}
