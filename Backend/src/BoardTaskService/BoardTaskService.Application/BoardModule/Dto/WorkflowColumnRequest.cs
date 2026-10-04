namespace BoardTaskService.Application.BoardModule.Dto;

/// <summary>
/// Represents the WorkflowColumnRequest component.
/// </summary>
public class WorkflowColumnRequest
{
    /// <summary>
    /// The unique identifier of an existing workflow column to update.
    /// <see langword="null"/> indicates a new workflow column should be created.
    /// </summary>
    public Guid? Id { get; set; }

    /// <summary>
    /// The display name of the workflow column.
    /// </summary>
    public string ColumnName { get; set; } = string.Empty;
    /// <summary>
    /// List of RoleId
    /// </summary>
    public List<Guid> RoleIds { get; set; } = [];
}

/// <summary>
/// Represents the full desired state of a board's workflow columns.
/// Columns with an <see cref="WorkflowColumnRequest.Id"/> are updated; columns without one are created;
/// existing columns not present in the list are removed.
/// </summary>
public class UpdateWorkflowColumnsRequest
{
    /// <summary>
    /// The complete ordered list of workflow columns to apply to the board.
    /// The order of items in this list determines each column's <see cref="WorkflowColumnDto.SortOrder"/>.
    /// </summary>
    public List<WorkflowColumnRequest> WorkflowColumnList { get; set; } = [];
}
