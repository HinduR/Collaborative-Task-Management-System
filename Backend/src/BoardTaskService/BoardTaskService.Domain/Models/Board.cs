using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Shared.Common.Model;

namespace BoardTaskService.Domain.Models;

public class Board : BaseModel
{
    [Key]
    public Guid Id { get; set; }

    public Guid ProjectId { get; set; }

    [Required]
    [MaxLength(150)]
    public required string Name { get; set; }

    [ForeignKey(nameof(ProjectId))]
    public Project? Project { get; set; }

    public ICollection<BoardAccess> BoardAccesses { get; set; } = [];

    public ICollection<WorkflowColumn> WorkflowColumns { get; set; } = [];
}