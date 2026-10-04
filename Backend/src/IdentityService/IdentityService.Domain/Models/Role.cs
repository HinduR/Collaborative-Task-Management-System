using System.ComponentModel.DataAnnotations;
using Shared.Common.Model;

namespace IdentityService.Domain.Models;

public class Role : BaseModel
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(100)]
    public required string Name { get; set; }
    public string? Description { get; set; }
    public ICollection<UserRoleMapping> UserRoleMappings { get; set; } = [];

    public ICollection<RoleFeatureMapping> RoleFeatureMappings { get; set; } = [];
}