using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Shared.Common.Model;

namespace BoardTaskService.Domain.Models;

public class UserProjectMapping : BaseModel
{
    [Key]
    public Guid Id { get; set; }

    public Guid ProjectId { get; set; }

    // External Identity Service user ID.
    // Do not configure a physical FK to Identity database.
    public Guid UserId { get; set; }

    [ForeignKey(nameof(ProjectId))]
    public Project? Project { get; set; }

    public ICollection<BoardAccess> BoardAccesses { get; set; } = [];
}