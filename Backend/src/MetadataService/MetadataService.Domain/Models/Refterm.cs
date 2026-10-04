using System.ComponentModel.DataAnnotations;
using Shared.Common.Model;

namespace MetadataService.Domain.Models;

public class RefTerm : BaseModel
{
    [Key]
    public Guid Id { get; set; }
    public string? RefTermKey { get; set; }
    public string? Description { get; set; }
}