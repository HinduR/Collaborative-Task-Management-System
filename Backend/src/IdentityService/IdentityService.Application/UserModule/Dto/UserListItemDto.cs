namespace IdentityService.Application.UserModule.Dto;

/// <summary>
/// Represents the UserListItemDto component.
/// </summary>
public class UserListItemDto
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

/// <summary>
/// Represents the UserListResponseDto component.
/// </summary>
public class UserListResponseDto
{
    /// <summary>
    /// Gets or sets the Items value.
    /// </summary>
    public List<UserListItemDto> Items { get; set; } = [];
}
