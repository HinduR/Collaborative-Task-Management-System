using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Shared.Common.Model;

namespace IdentityService.Domain.Models;

public class User : BaseModel
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(150)]
    public required string Name { get; set; }

    [Required]
    [MaxLength(255)]
    [EmailAddress]
    public required string Email { get; set; }
    public ICollection<UserRoleMapping> UserRoleMappings { get; set; } = [];

    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
}