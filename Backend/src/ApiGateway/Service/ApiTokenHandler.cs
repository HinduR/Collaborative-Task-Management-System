using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Shared.Grpc.Contracts;
using Shared.Logging.Contracts;
using Shared.SignalR.Constants;

namespace ApiGateway.YARP.Service;
/// <summary>
/// Reads the Bearer token from the request, validates it (format + expiry),
/// and builds a <see cref="ClaimsPrincipal"/> for downstream middleware.
/// JWT validation logic is consolidated here; a separate JwtTokenValidator
/// class is no longer needed as the gateway only performs a lightweight
/// format and expiry pre-check before forwarding to the Identity Service.
/// </summary>
public class ApiTokenHandler
    : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "ApiToken";

    private readonly ILoggerManager<ApiTokenHandler> _logger;
    private readonly IIdentityGrpcHelperService
        _identityHelperService;

    public ApiTokenHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory loggerFactory,
        UrlEncoder encoder,
        ILoggerManager<ApiTokenHandler> logger,
        IIdentityGrpcHelperService identityHelperService)
        : base(options, loggerFactory, encoder)
    {
        _logger = logger;
        _identityHelperService = identityHelperService;
    }

    /// <summary>
    /// Handles authentication by reading and validating the JWT token
    /// from the Authorization header and creating a <see cref="ClaimsPrincipal"/>.
    /// </summary>
    /// <returns>An <see cref="AuthenticateResult"/> indicating success or failure.</returns>

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        _logger.LogDebug(
            "Executing HandleAuthenticateAsync.");

        try
        {
            string? token = GetBearerToken();

            if (string.IsNullOrWhiteSpace(token))
            {
                return AuthenticateResult.NoResult();
            }

            if (string.IsNullOrWhiteSpace(token))
            {
                return AuthenticateResult.Fail(
                    "Bearer token is empty.");
            }

            JwtSecurityTokenHandler tokenHandler = new();

            if (!tokenHandler.CanReadToken(token))
            {
                return AuthenticateResult.Fail(
                    "Invalid token format.");
            }

            bool isValid = await _identityHelperService
                .ValidateAccessTokenAsync(
                    token,
                    Context.RequestAborted);

            if (!isValid)
            {
                return AuthenticateResult.Fail(
                    "Token rejected by Identity Service.");
            }

            JwtSecurityToken jwtToken =
       tokenHandler.ReadJwtToken(token);

            List<Claim> claims =
                jwtToken.Claims.ToList();

            string? userId =
                claims.FirstOrDefault(
                    claim =>
                        claim.Type ==
                        JwtRegisteredClaimNames.Sub)
                ?.Value;

            if (string.IsNullOrWhiteSpace(userId))
            {
                return AuthenticateResult.Fail(
                    "User ID claim is missing.");
            }

            if (!claims.Any(
                    claim =>
                        claim.Type ==
                        ClaimTypes.NameIdentifier))
            {
                claims.Add(
                    new Claim(
                        ClaimTypes.NameIdentifier,
                        userId));
            }

            ClaimsIdentity identity = new(
                claims,
                Scheme.Name,
                JwtRegisteredClaimNames.UniqueName,
                "role");

            ClaimsPrincipal principal = new(identity);

            return AuthenticateResult.Success(
                new AuthenticationTicket(
                    principal,
                    Scheme.Name));
        }
        catch (Exception exception)
        {
            _logger.LogError(
                "Gateway token authentication failed.",
                exception);

            return AuthenticateResult.Fail(
                "Token authentication failed.");
        }
    }

    private string? GetBearerToken()
    {
        string? authorization =
            Request.Headers.Authorization.FirstOrDefault();

        if (!string.IsNullOrWhiteSpace(authorization))
        {
            if (!authorization.StartsWith(
                    "Bearer ",
                    StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            return authorization["Bearer ".Length..].Trim();
        }

        if (Request.Path.StartsWithSegments(
                "/api/board-task/hubs/board") &&
            Request.Query.TryGetValue(
                SignalRConstants.AccessTokenQueryParameter,
                out Microsoft.Extensions.Primitives.StringValues tokenValues))
        {
            return tokenValues.FirstOrDefault();
        }

        return null;
    }
}
