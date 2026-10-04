using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Shared.Common.Model;

namespace IdentityService.Domain.Models;

public class RefreshToken : BaseModel
{
    [Key]
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    [Required]
    [MaxLength(255)]
    public required string GoogleSubjectId { get; set; }

    [Required]
    public required string TokenHash { get; set; }

    public DateTime ExpiryAt { get; set; }

    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }
}