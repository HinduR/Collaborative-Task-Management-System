using IdentityService.Application.Common;
using IdentityService.Application.UserModule.Contract.IService;
using IdentityService.Application.UserModule.Dto;
using IdentityService.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Shared.Exceptions.Infrastructure;
using Shared.Logging.Contracts;

namespace IdentityService.Infrastructure.UserModule.Service;

/// <summary>Provides operations for assigning roles to users.</summary>
public class UserRoleService : IUserRoleService
{
    private readonly IRepoWrapper _repoWrapper;
    private readonly ILoggerManager<UserRoleService> _logger;

    /// <summary>Initializes a new instance of the <see cref="UserRoleService"/> class.</summary>
    /// <param name="repoWrapper">Provides access to the required repositories.</param>
    /// <param name="logger">Logger used to record user-role assignment activity.</param>
    public UserRoleService(
        IRepoWrapper repoWrapper,
        ILoggerManager<UserRoleService> logger)
    {
        _repoWrapper = repoWrapper;
        _logger = logger;
    }

    /// <summary>Replaces the active role assigned to a user with the specified role.</summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="roleId">The unique identifier of the role to assign.</param>
    /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
    /// <returns>The resulting user-role mapping.</returns>
    public async Task<UserRoleMappingResponseDto>
        AssignRoleAsync(
            Guid userId,
            Guid roleId,
            CancellationToken cancellationToken)
    {
        _logger.LogDebug(
            "Executing AssignRoleAsync.");

        _logger.LogInformation(
            "Assigning role {RoleId} to user {UserId}.",
            roleId,
            userId);

        User? user =
            await _repoWrapper.UserRepository
                .FindFirstByConditionAsync(
                    currentUser =>
                        currentUser.IsActive &&
                        currentUser.Id == userId,
                    cancellationToken);

        if (user is null)
        {
            NotFoundCustomException exception = new(
                "The selected user does not exist.",
                "User not found.");

            _logger.LogError(
                "Role assignment failed because user {UserId} was not found.",
                exception,
                userId);

            throw exception;
        }

        Role? role =
            await _repoWrapper.RoleRepository
                .FindFirstByConditionAsync(
                    currentRole =>
                        currentRole.IsActive &&
                        currentRole.Id == roleId,
                    cancellationToken);

        if (role is null)
        {
            NotFoundCustomException exception = new(
                "The selected role does not exist.",
                "Role not found.");

            _logger.LogError(
                "Role assignment failed because role {RoleId} was not found.",
                exception,
                roleId);

            throw exception;
        }

        List<UserRoleMapping> activeMappings =
            await _repoWrapper
                .UserRoleMappingRepository
                .FindByCondition(mapping =>
                    mapping.IsActive &&
                    mapping.UserId == userId)
                .ToListAsync(cancellationToken);

        // The user already has exactly this role.
        if (activeMappings.Count == 1 &&
            activeMappings[0].RoleId == roleId)
        {
            _logger.LogInformation(
                "User {UserId} already has role {RoleId}. No role assignment change was required.",
                userId,
                roleId);

            return CreateResponse(
                activeMappings[0],
                user,
                role);
        }

        foreach (UserRoleMapping mapping
            in activeMappings)
        {
            mapping.IsActive = false;
        }

        if (activeMappings.Count > 0)
        {
            _repoWrapper.UserRoleMappingRepository
                .UpdateRange(activeMappings);
        }

        UserRoleMapping newMapping = new()
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            RoleId = role.Id,
            IsActive = true
        };

        await _repoWrapper
            .UserRoleMappingRepository
            .CreateAsync(
                newMapping,
                cancellationToken);

        await _repoWrapper.SaveChangesAsync(
            cancellationToken);

        _logger.LogInformation(
            "Role {RoleId} assigned to user {UserId}.",
            role.Id,
            user.Id);

        return CreateResponse(
            newMapping,
            user,
            role);
    }

    /// <summary>Maps a user-role mapping entity to its response DTO.</summary>
    /// <param name="mapping">The user-role mapping entity.</param>
    /// <param name="user">The user associated with the mapping.</param>
    /// <param name="role">The role associated with the mapping.</param>
    /// <returns>The mapped user-role response.</returns>
    private static UserRoleMappingResponseDto
        CreateResponse(
            UserRoleMapping mapping,
            User user,
            Role role)
    {
        return new UserRoleMappingResponseDto
        {
            MappingId = mapping.Id,
            UserId = user.Id,
            UserName = user.Name,
            RoleId = role.Id,
            RoleName = role.Name
        };
    }
}
