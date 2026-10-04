namespace BoardTaskService.Application.TaskModule.Dto;

/// <summary>
/// Represents the TaskDetailsDto component.
/// </summary>
public class TaskDetailsDto
{
    /// <summary>
    /// Gets or sets the Id value.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the Title value.
    /// </summary>
    public string Title { get; set; } = string.Empty;

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

    /// <summary>
    /// Gets or sets the Description value.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the Comments value.
    /// </summary>
    public List<TaskCommentDto> Comments { get; set; } = [];
}
