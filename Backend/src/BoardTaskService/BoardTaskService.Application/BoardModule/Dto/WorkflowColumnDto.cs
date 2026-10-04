namespace BoardTaskService.Application.BoardModule.Dto;
/// <summary>
/// Represents a workflow column belonging to a board, as returned to clients.
/// </summary>
public class WorkflowColumnDto
{
    /// <summary>
    /// The unique identifier of the workflow column.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The display name of the workflow column 
    /// </summary>
    public string ColumnName { get; set; } = string.Empty;
    /// <summary>
    /// The position of the column within the board, used to determine display order.
    /// </summary>
    public int SortOrder { get; set; }
    /// <summary>
    /// List of RoleId
    /// </summary>
    public List<Guid> RoleIds { get; set; } = [];

}
