using IdentityService.Application.AuthenticationModule.Contract.IService;
using MediatR;

namespace IdentityService.Application.AuthenticationModule.Command.Logout;

/// <summary>
/// Represents a request to log out the current user and revoke
/// the supplied refresh token.
/// </summary>
/// <param name="RefreshToken">
/// The refresh-token identifier that must be revoked.
/// </param>
public record LogoutCommand(
    Guid RefreshToken) : IRequest;

/// <summary>
/// Handles logout requests by delegating refresh-token revocation
/// to the login service.
/// </summary>
public class LogoutCommandHandler
    : IRequestHandler<LogoutCommand>
{
    private readonly ILoginService _loginService;

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="LogoutCommandHandler"/> class.
    /// </summary>
    public LogoutCommandHandler(
        ILoginService loginService)
    {
        _loginService = loginService;
    }

    /// <summary>
    /// Revokes the refresh token associated with the current session.
    /// </summary>
    public async Task Handle(
        LogoutCommand request,
        CancellationToken cancellationToken)
    {
        await _loginService.LogoutAsync(
            request.RefreshToken,
            cancellationToken);
    }
}
