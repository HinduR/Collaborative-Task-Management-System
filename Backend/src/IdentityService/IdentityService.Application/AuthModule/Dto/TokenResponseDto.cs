namespace IdentityService.Application.AuthenticationModule.Dto;

/// <summary>Represents the access token and refresh token generated after authentication or token rotation.</summary>
public class TokenResponseDto
{
    /// <summary>Gets or sets the generated access token.</summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>Gets or sets the generated refresh token.</summary>
    public Guid RefreshToken { get; set; } = Guid.Empty;

    /// <summary>Gets or sets the access-token lifetime in seconds.</summary>
    public int ExpiresIn { get; set; }
}
