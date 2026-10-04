using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Shared.Common.Model;

namespace BoardTaskService.Domain.Models;

public class BoardTask : BaseModel
{
    [Key]
    public Guid Id { get; set; }

    public Guid WorkflowColumnId { get; set; }

    public Guid? AssigneeBoardAccessId { get; set; }

    // Metadata Service ref_term IDs. No physical FK across databases.
    public Guid PriorityId { get; set; }

    // Metadata Service ref_term IDs. No physical FK across databases.
    public Guid TaskTypeId { get; set; }

    [Required]
    [MaxLength(200)]
    public required string Title { get; set; }

    public string? Description { get; set; }

    [ForeignKey(nameof(WorkflowColumnId))]
    public WorkflowColumn? WorkflowColumn { get; set; }

    [ForeignKey(nameof(AssigneeBoardAccessId))]
    public BoardAccess? AssigneeBoardAccess { get; set; }

    public ICollection<TaskComment> Comments { get; set; } = [];
}