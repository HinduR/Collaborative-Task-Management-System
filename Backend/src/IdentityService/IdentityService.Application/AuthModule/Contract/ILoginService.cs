using IdentityService.Application.AuthenticationModule.Dto;

namespace IdentityService.Application.AuthenticationModule.Contract.IService;

/// <summary>
/// Represents the ILoginService component.
/// </summary>
public interface ILoginService
{

    /// <summary>
    /// Generates a new access token using a valid refresh token.
    /// </summary>
    /// <param name="refreshToken">
    /// The refresh-token identifier.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the operation.
    /// </param>
    /// <returns>
    /// The newly generated application-token information.
    /// </returns>
    Task<TokenResponseDto> RefreshAccessTokenAsync(
       Guid refreshToken,
       CancellationToken cancellationToken);

    /// <summary>
    /// Creates a short-lived, one-time login code after successful
    /// Google authentication.
    /// </summary>
    /// <param name="googleUser">
    /// The user information received from Google authentication.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the operation.
    /// </param>
    /// <returns>
    /// The generated one-time login code.
    /// </returns>
    Task<string> CreateGoogleLoginCodeAsync(
        UserDto googleUser,
        CancellationToken cancellationToken);

    /// <summary>
    /// Exchanges a valid one-time Google login code for application tokens.
    /// </summary>
    /// <param name="code">
    /// The one-time Google login code.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the operation.
    /// </param>
    /// <returns>
    /// The generated access-token and refresh-token information.
    /// </returns>

    Task<TokenResponseDto> ExchangeGoogleLoginCodeAsync(
        string code,
        CancellationToken cancellationToken);

    /// <summary>
    /// Revokes the supplied refresh token and ends its session.
    /// </summary>
    /// <param name="refreshToken">The refresh-token identifier.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    Task LogoutAsync(
        Guid refreshToken,
        CancellationToken cancellationToken);
}
