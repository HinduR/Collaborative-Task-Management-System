namespace BoardTaskService.Application.TaskModule.Dto;

/// <summary>
/// Represents the TaskRequestDto component.
/// </summary>
public class TaskRequestDto
{
    /// <summary>
    /// Gets or sets the Title value.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Description value.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the PriorityRefTermKey value.
    /// </summary>
    public string PriorityRefTermKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the TaskTypeRefTermKey value.
    /// </summary>
    public string TaskTypeRefTermKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the AssigneeUserId value.
    /// </summary>
    public Guid? AssigneeUserId { get; set; }

    /// <summary>
    /// Gets or sets the WorkflowColumnId value.
    /// </summary>
    public Guid? WorkflowColumnId { get; set; }
}
