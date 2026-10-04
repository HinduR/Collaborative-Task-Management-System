using System.Security.Cryptography;
using IdentityService.Application.AuthenticationModule.Contract.IService;
using IdentityService.Application.AuthenticationModule.Dto;
using IdentityService.Application.Common;
using IdentityService.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Shared.Common.contracts;
using Shared.Cryptography.Application.Cryptography.Contract;
using Shared.Exceptions.Infrastructure;
using Shared.Logging.Contracts;

namespace IdentityService.Application.AuthenticationModule.Service;

/// <summary>
/// Represents the LoginService component.
/// </summary>
public class LoginService : ILoginService
{
    private readonly IRepoWrapper _repoWrapper;
    private readonly IAuthenticity _authenticity;
    private readonly IUserContext _userContext;
    private readonly JwtService _jwtService;
    private readonly IGoogleLoginCodeService _googleLoginCodeService;
    private readonly ILoggerManager<LoginService> _logger;
    private readonly int _accessDuration;
    private readonly int _refreshDuration;

    /// <summary>
    /// Initializes a new instance of the <see cref="LoginService"/> class.
    /// </summary>
    /// <param name="repoWrapper">Repository wrapper used for identity persistence operations.</param>
    /// <param name="configuration">Configuration source for JWT lifetime settings.</param>
    /// <param name="authenticity">Hashing service used to hash refresh tokens.</param>
    /// <param name="userContext">Current user context used when saving audit fields.</param>
    /// <param name="jwtService">Service used to create JWT access tokens.</param>
    /// <param name="googleLoginCodeService">Service used to create and consume one-time Google login codes.</param>
    /// <param name="logger">Logger used to record authentication activity.</param>
    public LoginService(
        IRepoWrapper repoWrapper,
        IConfiguration configuration,
        IAuthenticity authenticity,
        IUserContext userContext,
        JwtService jwtService,
        IGoogleLoginCodeService googleLoginCodeService,
        ILoggerManager<LoginService> logger)
    {
        _repoWrapper = repoWrapper;
        _jwtService = jwtService;
        _authenticity = authenticity;
        _userContext = userContext;
        _googleLoginCodeService = googleLoginCodeService;
        _logger = logger;

        _accessDuration = configuration.GetValue(
            "JwtConfig:DurationInMinutes",
            15);

        _refreshDuration = configuration.GetValue(
            "JwtConfig:RefreshDurationInMinutes",
            60);
    }

    /// <summary>
    /// Creates a short-lived, one-time login code after a Google principal
    /// has been validated by the API callback.
    /// </summary>
    /// <param name="googleUser">The user details resolved from the Google principal.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>The plaintext one-time code that must be returned only to the browser redirect.</returns>
    public async Task<string> CreateGoogleLoginCodeAsync(
        UserDto googleUser,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug(
            "Executing CreateGoogleLoginCodeAsync.");

        User user = await ValidateGoogleUserAsync(
            googleUser,
            cancellationToken);

        _logger.LogDebug(
            "Creating one-time Google login code for user {UserId}.",
            user.Id);

        string code =
            _googleLoginCodeService.CreateCode(
                new GoogleLoginCodeCacheDto
                {
                    UserId = user.Id,
                    GoogleSubjectId =
                        googleUser.GoogleSubjectId.Trim()
                });

        _userContext.SetUserId(user.Id);

        _logger.LogDebug(
            "Google login code created for user {UserId}.",
            user.Id);

        return code;
    }

    /// <summary>
    /// Consumes a one-time Google login code and creates the application
    /// access token and refresh token after successful consumption.
    /// </summary>
    /// <param name="code">The one-time code supplied by the Angular callback.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>The generated token response.</returns>
    public async Task<TokenResponseDto> ExchangeGoogleLoginCodeAsync(
        string code,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("Executing ExchangeGoogleLoginCodeAsync.");

        if (string.IsNullOrWhiteSpace(code))
        {
            _logger.LogDebug(
                "Google login-code exchange rejected because the code was missing.");

            throw new UnAuthorizedCustomException(
                "The login code is missing or invalid.",
                "Invalid login code.");
        }

        GoogleLoginCodeCacheDto? loginContext =
            _googleLoginCodeService.ConsumeCode(code);

        if (loginContext is null)
        {
            _logger.LogDebug(
                "Google login-code exchange rejected because the code was invalid, expired, or already used.");

            throw new UnAuthorizedCustomException(
                "The login code is invalid, expired, or already used.",
                "Invalid login code.");
        }

        _logger.LogDebug(
            "Google login code consumed for user {UserId}.",
            loginContext.UserId);

        User? user = await _repoWrapper.UserRepository
            .FindFirstByConditionAsync(
                item =>
                    item.IsActive &&
                    item.Id == loginContext.UserId,
                cancellationToken);

        if (user is null)
        {
            _logger.LogDebug(
                "Google login-code exchange rejected because user {UserId} is inactive or unavailable.",
                loginContext.UserId);

            throw new ForBiddenCustomException(
                "The user linked to this login code is inactive.",
                "User access is not available.");
        }

        TokenUserDto tokenUser =
            await GetTokenUserAsync(
                user,
                cancellationToken);

        TokenResponseDto response =
            await CreateTokenPairAsync(
                user.Id,
                loginContext.GoogleSubjectId,
                tokenUser,
                cancellationToken);

        _userContext.SetUserId(user.Id);

        await _repoWrapper.SaveChangesAsync(
            cancellationToken);

        _logger.LogDebug(
            "Google login code exchanged for user {UserId}.",
            user.Id);

        return response;
    }

    /// <summary>
    /// Rotates a valid refresh token and creates a new access-token response.
    /// </summary>
    /// <param name="refreshToken">The refresh-token identifier supplied from the secure cookie.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>The rotated token response.</returns>
    public async Task<TokenResponseDto> RefreshAccessTokenAsync(
        Guid refreshToken,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug(
            "Executing RefreshAccessTokenAsync.");

        string refreshTokenHash =
            HashRefreshToken(refreshToken);

        RefreshToken? savedRefreshToken =
            await _repoWrapper.RefreshTokenRepository
                .FindFirstByConditionAsync(
                    item =>
                        item.IsActive &&
                        item.TokenHash == refreshTokenHash &&
                        item.ExpiryAt > DateTime.Now,
                    cancellationToken);

        if (savedRefreshToken is null)
        {
            _logger.LogDebug(
                "Refresh-token rotation rejected because the token was invalid, inactive, or expired.");

            throw new BadRequestCustomException(
                "The refresh token is invalid or expired.",
                "Invalid refresh token.");
        }

        User? user = await _repoWrapper.UserRepository
            .FindFirstByConditionAsync(
                item =>
                    item.IsActive &&
                    item.Id == savedRefreshToken.UserId,
                cancellationToken);

        if (user is null)
        {
            _logger.LogDebug(
                "Refresh-token rotation rejected because user {UserId} is inactive or unavailable.",
                savedRefreshToken.UserId);

            throw new ForBiddenCustomException(
                "The user linked to this refresh token is inactive.",
                "User access is not available.");
        }

        TokenUserDto tokenUser =
            await GetTokenUserAsync(
                user,
                cancellationToken);

        _logger.LogDebug(
            "Revoking current refresh token for user {UserId} before rotation.",
            user.Id);

        // The old refresh token cannot be used again.
        savedRefreshToken.IsActive = false;

        TokenResponseDto response =
            await CreateTokenPairAsync(
                user.Id,
                savedRefreshToken.GoogleSubjectId,
                tokenUser,
                cancellationToken);

        _userContext.SetUserId(user.Id);

        await _repoWrapper.SaveChangesAsync(
            cancellationToken);

        _logger.LogDebug(
            "Access token refreshed for user {UserId}.",
            user.Id);

        return response;
    }

    /// <summary>
    /// Creates a JWT access token and persists a hashed refresh token.
    /// </summary>
    /// <param name="userId">The authenticated application user identifier.</param>
    /// <param name="googleSubjectId">The Google subject identifier linked to the user session.</param>
    /// <param name="tokenUser">The user claims used to generate the access token.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>The generated access token and plaintext refresh-token identifier.</returns>
    private async Task<TokenResponseDto> CreateTokenPairAsync(
      Guid userId,
      string googleSubjectId,
      TokenUserDto tokenUser,
      CancellationToken cancellationToken)
    {
        DateTime currentTime = DateTime.Now;

        _logger.LogDebug(
            "Creating token pair for user {UserId}. Access lifetime: {AccessDuration} minutes, refresh lifetime: {RefreshDuration} minutes.",
            userId,
            _accessDuration,
            _refreshDuration);

        string accessToken =
            _jwtService.GenerateAccessToken(
                tokenUser,
                currentTime.AddMinutes(_accessDuration));

        Guid refreshTokenValue = Guid.NewGuid();

        RefreshToken refreshTokenRecord = new()
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            GoogleSubjectId = googleSubjectId,
            TokenHash = HashRefreshToken(refreshTokenValue),
            ExpiryAt =
                currentTime.AddMinutes(_refreshDuration),
            IsActive = true
        };

        await _repoWrapper.RefreshTokenRepository
            .CreateAsync(
                refreshTokenRecord,
                cancellationToken);

        _logger.LogDebug(
            "Refresh token record created for user {UserId}. Refresh token expires at {ExpiryAt}.",
            userId,
            refreshTokenRecord.ExpiryAt);

        return new TokenResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshTokenValue,
            ExpiresIn = _accessDuration * 60
        };
    }

    /// <summary>
    /// Validates that the Google principal maps to an active configured user.
    /// </summary>
    /// <param name="googleUser">The Google-authenticated user details.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>The matching active application user.</returns>
    private async Task<User> ValidateGoogleUserAsync(
        UserDto googleUser,
        CancellationToken cancellationToken)
    {
        string email =
            googleUser.Email.Trim().ToLowerInvariant();

        string googleSubjectId =
            googleUser.GoogleSubjectId.Trim();

        _logger.LogDebug(
            "Validating Google user with email {Email}.",
            email);

        User? user = await _repoWrapper.UserRepository
            .FindFirstByConditionAsync(
                item =>
                    item.IsActive &&
                    item.Email.ToLower() == email,
                cancellationToken);

        if (user is null)
        {
            _logger.LogDebug(
                "Google user validation failed because email {Email} is not configured.",
                email);

            throw new ForBiddenCustomException(
                "Your account has not been configured.",
                "User access is not available.");
        }

        await ValidateGoogleSubjectAsync(
            user.Id,
            googleSubjectId,
            cancellationToken);

        _logger.LogDebug(
            "Google user validation completed for user {UserId}.",
            user.Id);

        return user;
    }

    /// <summary>
    /// Builds the JWT user payload from the user's active role and permissions.
    /// </summary>
    /// <param name="user">The active application user.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>The token user payload.</returns>
    private async Task<TokenUserDto> GetTokenUserAsync(
        User user,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug(
            "Building token user details for user {UserId}.",
            user.Id);

        UserRoleMapping? mapping =
            await _repoWrapper.UserRoleMappingRepository
                .FindFirstByConditionAsync(
                    item =>
                        item.IsActive &&
                        item.UserId == user.Id,
                    cancellationToken);

        if (mapping is null)
        {
            _logger.LogDebug(
                "Token user build failed because user {UserId} has no active role mapping.",
                user.Id);

            throw new ForBiddenCustomException(
                "The user does not have an assigned role.",
                "User role is required.");
        }

        Role? role =
            await _repoWrapper.RoleRepository
                .FindFirstByConditionAsync(
                    item =>
                        item.IsActive &&
                        item.Id == mapping.RoleId,
                    cancellationToken);

        if (role is null)
        {
            _logger.LogDebug(
                "Token user build failed because role {RoleId} for user {UserId} is inactive or unavailable.",
                mapping.RoleId,
                user.Id);

            throw new ForBiddenCustomException(
                "The assigned role is inactive or unavailable.",
                "Active role is required.");
        }

        List<Guid> featureIds =
            await _repoWrapper.RoleFeatureMappingRepository
                .FindByCondition(
                    item =>
                        item.IsActive &&
                        item.RoleId == role.Id)
                .Select(item => item.FeatureId)
                .Distinct()
                .ToListAsync(cancellationToken);

        List<string> permissions =
            await GetPermissionsAsync(
                featureIds,
                cancellationToken);

        _logger.LogDebug(
            "Token user details built for user {UserId}. Role: {RoleId}, permissions: {PermissionCount}.",
            user.Id,
            role.Id,
            permissions.Count);

        return new TokenUserDto
        {
            UserId = user.Id,
            UserName = user.Name,
            RoleId = role.Id,
            RoleName = role.Name,
            PermissionList = permissions
        };
    }

    /// <summary>
    /// Retrieves active permission keys for the supplied feature identifiers.
    /// </summary>
    /// <param name="featureIds">The feature identifiers mapped to the user's role.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>The distinct active permission keys.</returns>
    private async Task<List<string>> GetPermissionsAsync(
        List<Guid> featureIds,
        CancellationToken cancellationToken)
    {
        if (featureIds.Count == 0)
        {
            _logger.LogDebug(
                "No feature identifiers were found for the user's role.");

            return [];
        }

        List<string> permissions = await _repoWrapper.FeatureRepository
            .FindByCondition(
                item =>
                    item.IsActive &&
                    featureIds.Contains(item.Id))
            .Select(item => item.FeatureKey)
            .Distinct()
            .ToListAsync(cancellationToken);

        _logger.LogDebug(
            "Resolved {PermissionCount} permissions from {FeatureCount} features.",
            permissions.Count,
            featureIds.Count);

        return permissions;
    }

    /// <summary>
    /// Ensures the configured user is not being authenticated with a different
    /// Google account than the one previously used for refresh tokens.
    /// </summary>
    /// <param name="userId">The application user identifier.</param>
    /// <param name="googleSubjectId">The Google subject identifier from the current principal.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    private async Task ValidateGoogleSubjectAsync(
        Guid userId,
        string googleSubjectId,
        CancellationToken cancellationToken)
    {
        RefreshToken? previousToken =
            await _repoWrapper.RefreshTokenRepository
                .FindFirstByConditionAsync(
                    item => item.UserId == userId,
                    cancellationToken);

        if (previousToken is not null &&
            previousToken.GoogleSubjectId != googleSubjectId)
        {
            _logger.LogError(
                "Google account mismatch for user {UserId}.",
                null,
                userId);

            throw new ForBiddenCustomException(
                "This email is linked to another Google account.",
                "Google account mismatch.");
        }

        _logger.LogDebug(
            "Google subject validation completed for user {UserId}.",
            userId);
    }

    /// <summary>
    /// Hashes a refresh-token identifier before persistence or lookup.
    /// </summary>
    /// <param name="refreshToken">The plaintext refresh-token identifier.</param>
    /// <returns>The SHA-256 hash of the refresh token.</returns>
    private string HashRefreshToken(
        Guid refreshToken)
    {
        using SHA256 sha256 = SHA256.Create();

        return _authenticity.Hash(
            refreshToken.ToString("D"),
            sha256);
    }

    /// <summary>
    /// Revokes the refresh token associated with the current session.
    /// </summary>
    public async Task LogoutAsync(
        Guid refreshToken,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug(
            "Executing LogoutAsync.");

        string refreshTokenHash =
            HashRefreshToken(refreshToken);

        RefreshToken? existingRefreshToken =
            await _repoWrapper.RefreshTokenRepository
                .FindFirstByConditionAsync(
                    token =>
                        token.IsActive &&
                        token.TokenHash == refreshTokenHash,
                    cancellationToken);

        if (existingRefreshToken is null)
        {
            // Logout remains idempotent. An already expired, revoked, or
            // unknown token is treated as an already-ended session.
            _logger.LogError(
                "Logout requested with an inactive or unknown refresh token.");

            return;
        }

        existingRefreshToken.IsActive = false;

        await _repoWrapper.SaveChangesAsync(
            cancellationToken);

        _logger.LogDebug(
            "Refresh token revoked during logout for user {UserId}.",
            existingRefreshToken.UserId);
    }
}
