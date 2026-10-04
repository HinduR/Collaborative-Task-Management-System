namespace IdentityService.Application.AuthenticationModule.Dto;

/// <summary>Represents the user, role, and permission information included in an access token.</summary>
public class TokenUserDto
{
    /// <summary>Gets or sets the unique identifier of the user.</summary>
    public Guid UserId { get; set; }

    /// <summary>Gets or sets the display name of the user.</summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>Gets or sets the unique identifier of the user's role.</summary>
    public Guid RoleId { get; set; }

    /// <summary>Gets or sets the name of the user's role.</summary>
    public string RoleName { get; set; } = string.Empty;

    /// <summary>Gets or sets the permissions assigned to the user.</summary>
    public List<string> PermissionList { get; set; } = [];
}
