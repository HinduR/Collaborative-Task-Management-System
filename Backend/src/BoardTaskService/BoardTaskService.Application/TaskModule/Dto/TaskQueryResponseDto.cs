namespace BoardTaskService.Application.TaskModule.Dto;

/// <summary>
/// Represents the TaskQueryResponseDto component.
/// </summary>
public class TaskQueryResponseDto
{
    /// <summary>
    /// Gets or sets the Items value.
    /// </summary>
    public List<TaskDetailsDto> Items { get; set; } = [];

    /// <summary>
    /// Gets or sets the TotalCount value.
    /// </summary>
    public int TotalCount { get; set; }
}
