using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Shared.Common.Model;

namespace BoardTaskService.Domain.Models;

public class TaskComment : BaseModel
{
    [Key]
    public Guid Id { get; set; }

    public Guid TaskId { get; set; }

    [Required]
    public required string Comment { get; set; }

    [ForeignKey(nameof(TaskId))]
    public BoardTask? Task { get; set; }
}