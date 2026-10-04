namespace IdentityService.Application.AuthenticationModule.Dto;
/// <summary>
/// Represents the UserDto component.
/// </summary>
public class UserDto
{
    /// <summary>
    /// Gets or sets the GoogleSubjectId value.
    /// </summary>
    public string GoogleSubjectId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Email value.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Name value.
    /// </summary>
    public string Name { get; set; } = string.Empty;
}
