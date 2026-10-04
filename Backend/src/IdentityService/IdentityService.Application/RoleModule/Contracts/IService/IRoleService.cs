using IdentityService.Domain.Models;

namespace IdentityService.Application.RoleModule.Contract.IService;

public interface IRoleService
{
    Task<List<Role>> GetActiveRolesAsync(
        CancellationToken cancellationToken);
}