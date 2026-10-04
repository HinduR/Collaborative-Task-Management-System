using System.Security.Claims;
using IdentityService.Application.AuthenticationModule.Command;
using IdentityService.Application.AuthenticationModule.Command.Logout;
using IdentityService.Application.AuthenticationModule.Command.Refresh;
using IdentityService.Application.AuthenticationModule.Dto;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Logging.Contracts;

namespace IdentityService.API.Controllers.AuthenticationModule;

/// <summary>Provides Google authentication, token refresh, and logout endpoints.</summary>
[AllowAnonymous]
[ApiController]
[Route("identity/auth")]
/// <summary>
/// Represents the AuthController component.
/// </summary>
public class AuthController : ControllerBase
{
    private const string RefreshTokenCookieName =
        "round-table-refresh-token";

    private readonly IMediator _mediator;
    private readonly IConfiguration _configuration;
    private readonly ILoggerManager<AuthController> _logger;

    /// <summary>Initializes a new instance of the <see cref="AuthController"/> class.</summary>
    /// <param name="mediator">Mediator used to dispatch authentication commands.</param>
    /// <param name="configuration">Provides access to authentication and frontend configuration.</param>
    /// <param name="logger">Logger used to record authentication activity.</param>
    public AuthController(
        IMediator mediator,
        IConfiguration configuration,
        ILoggerManager<AuthController> logger)
    {
        _mediator = mediator;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>Starts the Google authentication flow and redirects the browser to Google.</summary>
    /// <returns>A Google authentication challenge response.</returns>
    [HttpGet("google/login")]
    public IActionResult GoogleLogin()
    {
        _logger.LogDebug(
            "Starting Google authentication flow.");

        AuthenticationProperties properties = new()
        {
            RedirectUri =
                "/api/identity/auth/google/success"
        };

        properties.SetParameter("prompt", "select_account");

        return Challenge(
            properties,
            GoogleDefaults.AuthenticationScheme);
    }

    /// <summary>Processes a successful Google authentication response, creates a login code, and redirects the browser to the frontend.</summary>
    /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
    /// <returns>A redirect to the auth callback page when authentication succeeds, or the login page when it fails.</returns>
    [HttpGet("google/success")]
    public async Task<IActionResult> GoogleSuccess(
        CancellationToken cancellationToken)
    {
        _logger.LogDebug(
            "Processing Google authentication callback.");

        AuthenticateResult result =
            await HttpContext.AuthenticateAsync(
                CookieAuthenticationDefaults
                    .AuthenticationScheme);

        if (!result.Succeeded ||
            result.Principal is null)
        {
            _logger.LogDebug(
                "Google authentication callback failed because the authentication principal was unavailable.");

            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults
                    .AuthenticationScheme);

            return RedirectToFrontend(
                "/login?error=google-authentication-failed");
        }

        ClaimsPrincipal principal = result.Principal;

        UserDto googleUser = new()
        {
            GoogleSubjectId =
                principal.FindFirstValue(
                    ClaimTypes.NameIdentifier)
                ?? string.Empty,

            Email =
                principal.FindFirstValue(
                    ClaimTypes.Email)
                ?? string.Empty,

            Name =
                principal.FindFirstValue(
                    ClaimTypes.Name)
                ?? string.Empty
        };

        _logger.LogDebug(
            "Google authentication succeeded for user {Email}. Creating one-time login code.",
            googleUser.Email);

        string loginCode =
            await _mediator.Send(
                new CreateGoogleLoginCodeCommand(googleUser),
                cancellationToken);

        // Delete only the temporary Google authentication cookie.
        await HttpContext.SignOutAsync(
            CookieAuthenticationDefaults
                .AuthenticationScheme);

        _logger.LogDebug(
            "One-time login code created for user {Email}. Redirecting to the frontend callback.",
            googleUser.Email);

        return RedirectToFrontend(
            $"/auth/callback?code={Uri.EscapeDataString(loginCode)}");
    }

    /// <summary>Exchanges a one-time Google login code for application tokens.</summary>
    /// <param name="request">The exchange request containing the one-time code.</param>
    /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
    /// <returns>The access token response and refresh-token cookie.</returns>
    [HttpPost("google/exchange")]
    public async Task<IActionResult> ExchangeGoogleLoginCode(
        [FromBody] GoogleLoginCodeExchangeRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug(
            "Processing Google login-code exchange request.");

        TokenResponseDto response =
            await _mediator.Send(
                new ExchangeGoogleLoginCodeCommand(request),
                cancellationToken);

        WriteRefreshTokenCookie(response);

        _logger.LogDebug(
            "Google login-code exchange completed successfully.");

        return Ok(
            CreateAccessTokenResponse(response));
    }

    /// <summary>Rotates the refresh token and creates a new access token.</summary>
    /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
    /// <returns>A 204 response when the token pair is refreshed, or a 401 response when the refresh token is missing or invalid.</returns>
    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshAccessToken(
        CancellationToken cancellationToken)
    {
        _logger.LogDebug(
            "Processing access-token refresh request.");

        if (!Request.Cookies.TryGetValue(
                RefreshTokenCookieName,
                out string? refreshTokenValue) ||
            !Guid.TryParse(
                refreshTokenValue,
                out Guid refreshToken))
        {
            _logger.LogDebug(
                "Access-token refresh rejected because the refresh-token cookie is missing or invalid.");

            return Unauthorized(
                "Refresh token is missing or invalid.");
        }

        RefreshAccessTokenRequest request = new()
        {
            RefreshToken = refreshToken
        };

        TokenResponseDto response =
            await _mediator.Send(
                new RefreshAccessTokenCommand(request),
                cancellationToken);

        WriteRefreshTokenCookie(response);

        _logger.LogDebug(
            "Access token and refresh token rotated successfully.");

        return Ok(CreateAccessTokenResponse(response));
    }

    /// <summary>Handles a cancelled or unsuccessful Google authentication attempt.</summary>
    /// <returns>A 401 response indicating that Google authentication failed.</returns>
    [HttpGet("google/failure")]
    public IActionResult GoogleFailure()
    {
        _logger.LogDebug(
            "Google authentication was cancelled or failed.");

        return Unauthorized(
            "Google authentication was cancelled or failed.");
    }

    /// <summary>Logs out the current user, revokes the refresh token, and removes the authentication cookies.</summary>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A 204 response when logout is completed.</returns>
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(
        CancellationToken cancellationToken)
    {
        _logger.LogDebug(
            "Processing logout request.");

        try
        {
            if (Request.Cookies.TryGetValue(
                    RefreshTokenCookieName,
                    out string? refreshTokenValue) &&
                Guid.TryParse(
                    refreshTokenValue,
                    out Guid refreshToken))
            {
                _logger.LogDebug(
                    "Revoking the current refresh token.");

                await _mediator.Send(
                    new LogoutCommand(refreshToken),
                    cancellationToken);

                _logger.LogDebug(
                    "Refresh token revoked successfully.");
            }
        }
        finally
        {
            DeleteTokenCookies();

            _logger.LogDebug(
                "Authentication cookies deleted during logout.");
        }

        return NoContent();
    }

    /// <summary>Writes the refresh-token cookie to the HTTP response.</summary>
    /// <param name="response">The generated access-token and refresh-token response.</param>
    private void WriteRefreshTokenCookie(
        TokenResponseDto response)
    {
        int refreshDuration =
            _configuration.GetValue(
                "JwtConfig:RefreshDurationInMinutes",
                60);

        Response.Cookies.Append(
            RefreshTokenCookieName,
            response.RefreshToken.ToString("D"),
            CreateRefreshTokenCookieOptions(
                refreshDuration));

        _logger.LogDebug(
            "Refresh-token cookie written. Refresh-token duration: {RefreshDuration} minutes.",
            refreshDuration);
    }

    /// <summary>Creates the cookie options used for the refresh-token cookie.</summary>
    /// <param name="durationInMinutes">The refresh-token cookie lifetime in minutes.</param>
    /// <returns>The configured refresh-token cookie options.</returns>
    private static CookieOptions CreateRefreshTokenCookieOptions(
        int durationInMinutes)
    {
        return new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Path = "/api/identity/auth",
            Expires = DateTimeOffset.Now.AddMinutes(
                durationInMinutes)
        };
    }

    /// <summary>Redirects the browser to a path in the configured Angular frontend.</summary>
    /// <param name="path">The frontend-relative path to redirect to.</param>
    /// <returns>A redirect response to the configured frontend URL.</returns>
    private IActionResult RedirectToFrontend(
        string path)
    {
        string frontendBaseUrl =
            _configuration["FrontendConfig:BaseUrl"]
            ?? throw new InvalidOperationException(
                "FrontendConfig:BaseUrl is not configured.");

        _logger.LogDebug(
            "Redirecting browser to frontend path {Path}.",
            path);

        return Redirect(
            $"{frontendBaseUrl.TrimEnd('/')}{path}");
    }

    /// <summary>Creates the JSON access-token response body.</summary>
    /// <param name="response">The generated token response.</param>
    /// <returns>The response body returned to Angular.</returns>
    private static object CreateAccessTokenResponse(
        TokenResponseDto response)
    {
        return new
        {
            accessToken = response.AccessToken,
            expiresIn = response.ExpiresIn
        };
    }

    /// <summary>Deletes all authentication cookies from the browser.</summary>
    private void DeleteTokenCookies()
    {
        DeleteCookie(
            RefreshTokenCookieName,
            "/api/identity/auth");

        // Remove the legacy refresh-token cookie created
        // using the previous path.
        DeleteCookie(
            RefreshTokenCookieName,
            "/api/identity/auth/refresh");
    }

    /// <summary>Appends an expired cookie using the supplied cookie name and path.</summary>
    /// <param name="cookieName">The name of the cookie to delete.</param>
    /// <param name="path">The path associated with the cookie.</param>
    private void DeleteCookie(
        string cookieName,
        string path)
    {
        Response.Cookies.Append(
            cookieName,
            string.Empty,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Path = path,
                Expires = DateTimeOffset.UnixEpoch,
                MaxAge = TimeSpan.Zero
            });

        _logger.LogDebug(
            "Authentication cookie {CookieName} deleted for path {CookiePath}.",
            cookieName,
            path);
    }
}
