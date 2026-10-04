using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Shared.Common.Model;

namespace BoardTaskService.Domain.Models;

public class Project : BaseModel
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(150)]
    public required string Name { get; set; }

    public ICollection<UserProjectMapping> UserProjectMappings { get; set; } = [];

    public ICollection<Board> Boards { get; set; } = [];
}