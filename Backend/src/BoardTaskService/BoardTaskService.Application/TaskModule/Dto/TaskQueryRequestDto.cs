namespace BoardTaskService.Application.TaskModule.Dto;

/// <summary>
/// Represents the TaskQueryRequestDto component.
/// </summary>
public class TaskQueryRequestDto
{
    /// <summary>
    /// Gets or sets the Conditions value.
    /// </summary>
    public List<TaskQueryConditionDto> Conditions { get; set; } = [];

    /// <summary>
    /// Gets or sets the PageNumber value.
    /// </summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Gets or sets the PageSize value.
    /// </summary>
    public int PageSize { get; set; } = 20;
}
