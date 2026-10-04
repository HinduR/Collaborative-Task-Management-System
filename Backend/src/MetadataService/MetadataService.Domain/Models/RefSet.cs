using System.ComponentModel.DataAnnotations;
using Shared.Common.Model;

namespace MetadataService.Domain.Models;

public class RefSet : BaseModel
{
    [Key]
    public Guid Id { get; set; }

    public string? RefSetKey { get; set; }

    public string? Description { get; set; }
}