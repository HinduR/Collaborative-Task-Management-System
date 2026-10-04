using IdentityService.Application.AuthenticationModule.Dto;

namespace IdentityService.Application.AuthenticationModule.Contract.IService;

/// <summary>
/// Defines operations for creating and consuming short-lived,
/// one-time Google login codes.
/// </summary>
public interface IGoogleLoginCodeService
{
    /// <summary>
    /// Creates and stores a one-time login code for the supplied
    /// Google login context.
    /// </summary>
    /// <param name="loginContext">
    /// The Google-authenticated user context associated with the code.
    /// </param>
    /// <returns>
    /// The generated one-time login code.
    /// </returns>
    string CreateCode(
        GoogleLoginCodeCacheDto loginContext);

    /// <summary>
    /// Consumes and invalidates the specified one-time login code.
    /// </summary>
    /// <param name="code">
    /// The one-time login code to consume.
    /// </param>
    /// <returns>
    /// The cached Google login context when the code is valid;
    /// otherwise, <see langword="null"/>.
    /// </returns>
    GoogleLoginCodeCacheDto? ConsumeCode(
        string code);
}