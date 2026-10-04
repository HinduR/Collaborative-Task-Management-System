using Shared.Authorisation.Domain.Contract;
using Shared.Common.contracts;
using Shared.Logging.Contracts;

namespace Shared.Authorisation.Application.Service;

/// <summary>Provides permission checks using the permissions stored in the current user context.</summary>
public class AuthorizationService : IAuthorizationService
{
    private readonly IUserContext _userContext;
    private readonly ILoggerManager<AuthorizationService> _logger;

    /// <summary>Initializes a new instance of the <see cref="AuthorizationService"/> class.</summary>
    /// <param name="userContext">Provides the current authenticated user's permissions.</param>
    /// <param name="logger">Logger used to record authorization checks.</param>
    public AuthorizationService(
        IUserContext userContext,
        ILoggerManager<AuthorizationService> logger)
    {
        _userContext = userContext;
        _logger = logger;
    }

    /// <summary>Determines whether the current user has the supplied API permission.</summary>
    /// <param name="apiKey">The permission key required by the API.</param>
    /// <returns><see langword="true"/> when the permission is assigned; otherwise, <see langword="false"/>.</returns>
    public bool HasPermission(string apiKey)
    {
        _logger.LogDebug(
            "Executing HasPermission.");

        bool hasPermission = _userContext
            .GetPermissions()
            .Contains(
                apiKey,
                StringComparer.OrdinalIgnoreCase);

        _logger.LogInformation(
            "Permission check completed for API key {ApiKey}. Granted: {HasPermission}.",
            apiKey,
            hasPermission);

        return hasPermission;
    }
}
