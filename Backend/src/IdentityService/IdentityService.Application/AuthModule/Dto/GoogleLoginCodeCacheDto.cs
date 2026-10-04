namespace IdentityService.Application.AuthenticationModule.Dto;

/// <summary>
/// Represents the user context temporarily associated
/// with a one-time Google login code.
/// </summary>
public class GoogleLoginCodeCacheDto
{
    /// <summary>
    /// Gets or sets the application's unique user identifier.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Gets or sets the unique subject identifier
    /// provided by Google.
    /// </summary>
    public string GoogleSubjectId { get; set; } =
        string.Empty;
}