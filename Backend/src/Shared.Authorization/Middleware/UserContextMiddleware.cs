using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Shared.Common.contracts;
using Shared.Logging.Contracts;

namespace Shared.Authorization.Middleware;

/// <summary>
/// Middleware that reads authenticated user claims from the HTTP context
/// and populates <see cref="IUserContext"/> for downstream usage.
/// </summary>
public class UserContextMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILoggerManager<UserContextMiddleware> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserContextMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <param name="logger">Shared logger instance.</param>
    public UserContextMiddleware(RequestDelegate next, ILoggerManager<UserContextMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// Invokes the middleware to populate user context if the request is authenticated.
    /// </summary>
    /// <param name="context">The current HTTP context.</param>
    /// <param name="userContext">The user context to populate.</param>
    public async Task InvokeAsync(HttpContext context, IUserContext userContext)
    {
        HydrateRequestContext(context, userContext);

        if (context.User.Identity?.IsAuthenticated == true && context.User.Claims.Any())
        {
            HydrateUserContext(context, userContext);
        }
        else
        {
            _logger.LogDebug(
                "Skipping user context hydration because request is not authenticated or has no claims. Authenticated: {Auth}, Claims Count: {Count}",
                context.User.Identity?.IsAuthenticated,
                context.User.Claims.Count());
        }

        await _next(context);
    }

    /// <summary>
    /// Extracts user-related claims from the authenticated principal
    /// and sets them into the <see cref="IUserContext"/>.
    /// Falls back to sensible defaults when a claim is absent.
    /// </summary>
    /// <param name="context">The current HTTP context.</param>
    /// <param name="userContext">The user context to populate.</param>
    private void HydrateUserContext(
    HttpContext context,
    IUserContext userContext)
    {
        ClaimsPrincipal principal = context.User;

        string? userIdValue =
            principal.FindFirstValue("sub")
            ?? principal.FindFirstValue(
                ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(
                userIdValue,
                out Guid userId))
        {
            _logger.LogError(
                "The JWT does not contain a valid sub claim.");

            return;
        }

        string? roleIdValue =
            principal.FindFirstValue("role_id");

        Guid roleId = Guid.TryParse(
            roleIdValue,
            out Guid parsedRoleId)
                ? parsedRoleId
                : Guid.Empty;

        string userName =
            principal.FindFirstValue("unique_name")
            ?? principal.FindFirstValue(
                ClaimTypes.Name)
            ?? "Unknown";

        string roleName =
            principal.FindFirstValue("role")
            ?? principal.FindFirstValue(
                ClaimTypes.Role)
            ?? "Unknown";

        List<string> permissions =
            principal.FindAll("permission")
                .Select(claim => claim.Value)
                .Distinct()
                .ToList();

        userContext.SetUserId(userId);
        userContext.SetRoleId(roleId);
        userContext.SetUserName(userName);
        userContext.SetRoleName(roleName);
        userContext.SetPermissions(permissions);

        _logger.LogDebug(
            "User context hydrated. " +
            "UserId: {UserId}, RoleId: {RoleId}, " +
            "UserName: {UserName}, RoleName: {RoleName}",
            userId,
            roleId,
            userName,
            roleName);
    }
    private void HydrateRequestContext(HttpContext context, IUserContext userContext)
    {
        string? authorization = context.Request.Headers.Authorization.FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(authorization))
        {
            userContext.SetToken(authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                ? authorization["Bearer ".Length..].Trim()
                : authorization);
        }
    }
}
