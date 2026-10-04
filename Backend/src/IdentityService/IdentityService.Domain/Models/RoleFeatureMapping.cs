using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Shared.Common.Model;

namespace IdentityService.Domain.Models;

public class RoleFeatureMapping : BaseModel
{
    [Key]
    public Guid Id { get; set; }

    public Guid RoleId { get; set; }

    public Guid FeatureId { get; set; }
    public Role? Role { get; set; }
    public Feature? Feature { get; set; }
}