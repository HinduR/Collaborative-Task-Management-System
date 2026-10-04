namespace IdentityService.Application.AuthenticationModule.Contract.IService;

/// <summary>Defines authentication and access-token validation operations.</summary>
public interface IAuthenticationService
{
    /// <summary>Validates the supplied access token.</summary>
    /// <param name="accessToken">The access token to validate.</param>
    /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
    /// <returns><see langword="true"/> when the access token is valid; otherwise, <see langword="false"/>.</returns>
    Task<bool> ValidateAccessTokenAsync(
        string accessToken,
        CancellationToken cancellationToken);
}
