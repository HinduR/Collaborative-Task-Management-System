namespace BoardTaskService.Application.TaskModule.Dto;

/// <summary>
/// Represents the TaskQueryConditionDto component.
/// </summary>
public class TaskQueryConditionDto
{
    /// <summary>
    /// Gets or sets the LogicalOperator value.
    /// </summary>
    public string LogicalOperator { get; set; } = "AND";

    /// <summary>
    /// Gets or sets the Field value.
    /// </summary>
    public string Field { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Operator value.
    /// </summary>
    public string Operator { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Value value.
    /// </summary>
    public string Value { get; set; } = string.Empty;
}
