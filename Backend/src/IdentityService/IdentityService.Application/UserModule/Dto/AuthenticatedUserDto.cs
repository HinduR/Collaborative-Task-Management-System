namespace IdentityService.Application.UserModule.Dto;

/// <summary>
/// Represents the AuthenticatedUserDto component.
/// </summary>
public class AuthenticatedUserDto
{
    /// <summary>
    /// Gets or sets the Id value.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the Email value.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the UserName value.
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Roles value.
    /// </summary>
    public List<string> Roles { get; set; } = [];

    /// <summary>
    /// Gets or sets the Features value.
    /// </summary>
    public List<string> Features { get; set; } = [];
}
