namespace IdentityService.Application.AuthenticationModule.Dto;

/// <summary>
/// Represents a request to exchange a short-lived,
/// one-time Google login code for application tokens.
/// </summary>
public class GoogleLoginCodeExchangeRequest
{
    /// <summary>
    /// Gets or sets the one-time Google login code.
    /// </summary>
    public string Code { get; set; } = string.Empty;
}