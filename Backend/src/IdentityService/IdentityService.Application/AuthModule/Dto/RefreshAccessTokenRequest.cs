namespace IdentityService.Application.AuthenticationModule.Dto;

/// <summary>Represents a request to rotate an existing refresh token.</summary>
public class RefreshAccessTokenRequest
{
    /// <summary>Gets or sets the refresh token to rotate.</summary>
    public Guid RefreshToken { get; set; }
}
