using IdentityService.Application.Common;
using IdentityService.Application.UserModule.Contract.IService;
using IdentityService.Application.UserModule.Dto;
using IdentityService.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Shared.Common.contracts;
using Shared.Exceptions.Infrastructure;
using Shared.Logging.Contracts;

namespace IdentityService.Infrastructure.UserModule.Service;

/// <summary>Provides operations for retrieving users and their authorization details.</summary>
public class UserService : IUserService
{
    private readonly IRepoWrapper _repoWrapper;
    private readonly IUserContext _userContext;
    private readonly ILoggerManager<UserService> _logger;

    /// <summary>Initializes a new instance of the <see cref="UserService"/> class.</summary>
    /// <param name="repoWrapper">Provides access to the required repositories.</param>
    /// <param name="userContext">Provides information about the current authenticated user.</param>
    /// <param name="logger">Logger used to record user retrieval activity.</param>
    public UserService(
        IRepoWrapper repoWrapper,
        IUserContext userContext,
        ILoggerManager<UserService> logger)
    {
        _repoWrapper = repoWrapper;
        _userContext = userContext;
        _logger = logger;
    }

    /// <summary>Retrieves active users, optionally filtered by name or email address.</summary>
    /// <param name="search">The optional search text used to filter users.</param>
    /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
    /// <returns>A response containing the matching active users.</returns>
    public async Task<UserListResponseDto> GetUsers(
        string? search,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug(
            "Executing GetUsers.");


        IQueryable<User> query = _repoWrapper.UserRepository
            .FindByCondition(user => user.IsActive);

        if (!string.IsNullOrWhiteSpace(search))
        {
            string searchPattern = $"%{search.Trim()}%";

            query = query.Where(user =>
                EF.Functions.ILike(user.Name, searchPattern) ||
                EF.Functions.ILike(user.Email, searchPattern));
        }

        List<UserListItemDto> users = await query
            .OrderBy(user => user.Name)
            .Select(user => new UserListItemDto
            {
                Id = user.Id,
                Email = user.Email,
                DisplayName = user.Name
            })
            .ToListAsync(cancellationToken);

        return new UserListResponseDto
        {
            Items = users
        };
    }

    /// <summary>Retrieves a user and their active roles and features.</summary>
    /// <param name="userId">The optional user identifier. When omitted, the authenticated user's identifier is used.</param>
    /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
    /// <returns>The authenticated user details, roles, and features.</returns>
    public async Task<AuthenticatedUserDto> GetUserById(
        Guid? userId,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug(
            "Executing GetUserById.");

        Guid resolvedUserId = userId ?? _userContext.GetUserId();


        if (resolvedUserId == Guid.Empty)
        {
            UnAuthorizedCustomException exception = new(
                "The authenticated user could not be identified.",
                "User is not authenticated.");

            _logger.LogError(
                "User retrieval failed because the authenticated user ID was unavailable.",
                exception);

            throw exception;
        }

        User? user = await _repoWrapper.UserRepository
            .FindFirstByConditionAsync(
                item =>
                    item.IsActive &&
                    item.Id == resolvedUserId,
                cancellationToken);

        if (user is null)
        {
            NotFoundCustomException exception = new(
                "The selected user does not exist.",
                "User not found.");

            _logger.LogError(
                "User retrieval failed because user {UserId} was not found.",
                exception,
                resolvedUserId);

            throw exception;
        }

        List<Guid> roleIds = await _repoWrapper
            .UserRoleMappingRepository
            .FindByCondition(mapping =>
                mapping.IsActive &&
                mapping.UserId == resolvedUserId)
            .Select(mapping => mapping.RoleId)
            .ToListAsync(cancellationToken);

        List<Role> roles = await _repoWrapper.RoleRepository
            .FindByCondition(role =>
                role.IsActive &&
                roleIds.Contains(role.Id))
            .ToListAsync(cancellationToken);

        List<Guid> featureIds = await _repoWrapper
            .RoleFeatureMappingRepository
            .FindByCondition(mapping =>
                mapping.IsActive &&
                roleIds.Contains(mapping.RoleId))
            .Select(mapping => mapping.FeatureId)
            .Distinct()
            .ToListAsync(cancellationToken);

        List<string> featureKeys = await _repoWrapper.FeatureRepository
            .FindByCondition(feature =>
                feature.IsActive &&
                featureIds.Contains(feature.Id))
            .OrderBy(feature => feature.FeatureKey)
            .Select(feature => feature.FeatureKey)
            .ToListAsync(cancellationToken);

        _logger.LogDebug(
            "User {UserId} retrieved with {RoleCount} roles and {FeatureCount} features.",
            resolvedUserId,
            roles.Count,
            featureKeys.Count);

        return new AuthenticatedUserDto
        {
            Id = user.Id,
            Email = user.Email,
            UserName = user.Name,
            Roles = roles
                .Select(role => role.Name)
                .OrderBy(roleName => roleName)
                .ToList(),
            Features = featureKeys
        };
    }

    /// <summary>Retrieves active users matching the supplied identifiers in the requested order.</summary>
    /// <param name="userIdList">The user identifiers to retrieve.</param>
    /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
    /// <returns>A list containing the matching active users.</returns>
    public async Task<List<UserListItemDto>> GetUserListByIdAsync(
        List<Guid> userIdList,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug(
            "Executing GetUserListByIdAsync.");


        List<Guid> distinctUserIdList = userIdList
            .Where(userId => userId != Guid.Empty)
            .Distinct()
            .ToList();

        if (!distinctUserIdList.Any())
        {
            _logger.LogDebug(
                "No valid user identifiers were supplied.");

            return [];
        }

        List<UserListItemDto> userList = await _repoWrapper.UserRepository
            .FindByCondition(user =>
                user.IsActive &&
                distinctUserIdList.Contains(user.Id))
            .Select(user => new UserListItemDto
            {
                Id = user.Id,
                Email = user.Email,

                // Use user.DisplayName here instead if that is
                // the actual property name in your User entity.
                DisplayName = user.Name
            })
            .ToListAsync(cancellationToken);

        // Return users in the same order requested by BoardTask Service.
        Dictionary<Guid, UserListItemDto> userById = userList.ToDictionary(
            user => user.Id);

        _logger.LogDebug(
            "Fetched {UserCount} active users for {RequestedUserCount} distinct identifiers.",
            userList.Count,
            distinctUserIdList.Count);

        return distinctUserIdList
            .Where(userById.ContainsKey)
            .Select(userId => userById[userId])
            .ToList();
    }
}
