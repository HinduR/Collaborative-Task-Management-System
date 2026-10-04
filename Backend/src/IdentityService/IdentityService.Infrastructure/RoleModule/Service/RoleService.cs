using IdentityService.Application.Common;
using IdentityService.Application.RoleModule.Contract.IService;
using IdentityService.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Shared.Logging.Contracts;

namespace IdentityService.Infrastructure.RoleModule.Service;

/// <summary>
/// Handles role data access. Contains no business rules.
/// </summary>
public class RoleService : IRoleService
{
    private readonly IRepoWrapper _repoWrapper;
    private readonly ILoggerManager<RoleService> _logger;

    public RoleService(
        IRepoWrapper repoWrapper,
        ILoggerManager<RoleService> logger)
    {
        _repoWrapper = repoWrapper;
        _logger = logger;
    }

    public async Task<List<Role>> GetActiveRolesAsync(
        CancellationToken cancellationToken)
    {
        _logger.LogDebug(
            "Fetching active roles.");

        List<Role> roles = await _repoWrapper.RoleRepository
            .FindByCondition(role => role.IsActive)
            .OrderBy(role => role.Name)
            .ToListAsync(cancellationToken);

        _logger.LogDebug(
            "Fetched {RoleCount} active roles.",
            roles.Count);

        return roles;
    }
}