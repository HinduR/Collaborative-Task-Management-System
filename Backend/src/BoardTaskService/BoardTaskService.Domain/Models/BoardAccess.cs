using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Shared.Common.Model;

namespace BoardTaskService.Domain.Models;

public class BoardAccess : BaseModel
{
    [Key]
    public Guid Id { get; set; }

    public Guid BoardId { get; set; }

    public Guid UserProjectMappingId { get; set; }

    [ForeignKey(nameof(BoardId))]
    public Board? Board { get; set; }

    [ForeignKey(nameof(UserProjectMappingId))]
    public UserProjectMapping? UserProjectMapping { get; set; }

    public ICollection<BoardTask> AssignedTasks { get; set; } = [];
}