using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Shared.Common.Model;

namespace BoardTaskService.Domain.Models;
/// <summary>
/// Represents a workflow column within a board, such as
/// To Do, In Progress, or Completed.
/// </summary>
public class WorkflowColumn : BaseModel
{
    /// <summary>
    /// Gets or sets the unique identifier of the workflow column.
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the board containing this column.
    /// </summary>
    public Guid BoardId { get; set; }

    /// <summary>
    /// Gets or sets the display name of the workflow column.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets the display position of the column within the board.
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// Gets or sets the board containing this workflow column.
    /// </summary>
    [ForeignKey(nameof(BoardId))]
    public Board? Board { get; set; }

    /// <summary>
    /// Gets or sets the identifiers of the roles permitted to use
    /// this workflow column.
    /// </summary>
    public Guid[] RoleId { get; set; } = [];

    /// <summary>
    /// Gets or sets the tasks currently associated with this
    /// workflow column.
    /// </summary>
    public ICollection<BoardTask> Tasks { get; set; } = [];
}