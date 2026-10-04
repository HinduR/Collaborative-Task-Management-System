namespace BoardTaskService.Application.TaskModule.Dto;

/// <summary>
/// Represents the TaskSummaryDto component.
/// </summary>
public class TaskSummaryDto
{
    /// <summary>
    /// Gets or sets the Id value.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the WorkflowColumnId value.
    /// </summary>
    public Guid WorkflowColumnId { get; set; }

    /// <summary>
    /// Gets or sets the Title value.
    /// </summary>
    public string Title { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the Description value.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the PriorityName value.
    /// </summary>
    public string PriorityName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the TaskTypeName value.
    /// </summary>
    public string TaskTypeName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the AssigneeUserId value.
    /// </summary>
    public Guid? AssigneeUserId { get; set; }

    /// <summary>
    /// Gets or sets the AssigneeName value.
    /// </summary>
    public string? AssigneeName { get; set; }

    /// <summary>
    /// Gets or sets the IsTaskOwner value.
    /// </summary>
    public bool IsTaskOwner { get; set; }
}
