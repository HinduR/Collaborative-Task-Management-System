namespace Shared.Grpc.Common.Dto;

/// <summary>
/// Represents the UserDto component.
/// </summary>
public class UserDto
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
    /// Gets or sets the DisplayName value.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;
}
