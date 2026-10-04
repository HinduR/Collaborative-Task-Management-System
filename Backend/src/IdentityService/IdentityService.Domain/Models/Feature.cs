using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Shared.Common.Model;

namespace IdentityService.Domain.Models;

public class Feature : BaseModel
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(100)]
    public required string FeatureKey { get; set; }

    public string? Description { get; set; }

    public ICollection<RoleFeatureMapping> RoleFeatureMappings { get; set; } = [];
}