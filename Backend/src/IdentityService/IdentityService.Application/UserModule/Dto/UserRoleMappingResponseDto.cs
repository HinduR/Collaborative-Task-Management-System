namespace IdentityService.Application.UserModule.Dto;

/// <summary>
/// Represents the UserRoleMappingResponseDto component.
/// </summary>
public class UserRoleMappingResponseDto
{
    /// <summary>
    /// Gets or sets the MappingId value.
    /// </summary>
    public Guid MappingId { get; set; }

    /// <summary>
    /// Gets or sets the UserId value.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Gets or sets the UserName value.
    /// </summary>
    public string UserName { get; set; } =
        string.Empty;

    /// <summary>
    /// Gets or sets the RoleId value.
    /// </summary>
    public Guid RoleId { get; set; }

    /// <summary>
    /// Gets or sets the RoleName value.
    /// </summary>
    public string RoleName { get; set; } =
        string.Empty;
}
