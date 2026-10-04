using Microsoft.Extensions.Hosting;
using IdentityService.Application.AuthenticationModule.Contract.IService;
using IdentityService.Application.Common;
using Microsoft.Extensions.Configuration;
using Shared.Logging.Contracts;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using IdentityService.Application.AuthenticationModule.Service;

namespace IdentityService.Infrastructure.AuthenticationModule.Service;

/// <summary>
/// Represents the AuthenticationService component.
/// </summary>
public class AuthenticationService : IAuthenticationService
{
    private readonly IRepoWrapper _repoWrapper;
    private readonly JwtService _jwtService;
    private readonly ILoggerManager<AuthenticationService> _logger;

    public AuthenticationService(
        IRepoWrapper repoWrapper,
        JwtService jwtService,
        ILoggerManager<AuthenticationService> logger)
    {
        _repoWrapper = repoWrapper;
        _jwtService = jwtService;
        _logger = logger;
    }


    public async Task<bool> ValidateAccessTokenAsync(
    string accessToken,
    CancellationToken cancellationToken)
{
    _logger.LogDebug(
        "Executing ValidateAccessTokenAsync.");

    ClaimsPrincipal claimsPrincipal;

    try
    {
        claimsPrincipal = _jwtService.ValidateAccessToken(accessToken);
    }
    catch (SecurityTokenException exception)
    {
        _logger.LogError(
            "Access token validation failed: {Message}",
            exception);

        return false;
    }
    catch (ArgumentException exception)
    {
        _logger.LogError(
            "Access token format is invalid: {Message}",
            exception);

        return false;
    }

    string? userIdValue = claimsPrincipal
        .FindFirst("user_id")?.Value
        ?? claimsPrincipal
            .FindFirst(ClaimTypes.NameIdentifier)?.Value
        ?? claimsPrincipal
            .FindFirst("sub")?.Value;

    if (!Guid.TryParse(userIdValue, out Guid userId))
    {
        _logger.LogError(
            "Access token does not contain a valid user ID.");

        return false;
    }

    bool isUserActive = await _repoWrapper.UserRepository
        .AnyByConditionAsync(
            user => user.IsActive && user.Id == userId,
            cancellationToken);

    if (!isUserActive)
    {
        _logger.LogError(
            "Access token belongs to an inactive user {UserId}.",
            null,
            userId);

        return false;
    }

    return true;
}
}
