namespace BoardTaskService.Application.TaskModule.Dto;

/// <summary>
/// Represents the TaskCommentDto component.
/// </summary>
public class TaskCommentDto
{
    /// <summary>
    /// Gets or sets the Id value.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the Comment value.
    /// </summary>
    public string Comment { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the CreatedByName value.
    /// </summary>
    public string CreatedByName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the IsCommentOwner value.
    /// </summary>
    public bool IsCommentOwner { get; set; }
}
