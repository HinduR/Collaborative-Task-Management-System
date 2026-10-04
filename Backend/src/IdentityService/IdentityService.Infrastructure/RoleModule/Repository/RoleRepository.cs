using IdentityService.Application.RoleModule.Contract.IRepository;
using IdentityService.Domain.Models;
using IdentityService.Infrastructure.Persistence.ApplicationContext;
using Shared.Common.Repository;

namespace IdentityService.Infrastructure.RoleModule.Repository;

/// <summary>
/// Represents the RoleRepository component.
/// </summary>
public class RoleRepository(
    IdentityDbContext repositoryContext)
    : RepositoryBase<Role, IdentityDbContext>(repositoryContext),
      IRoleRepository
{
}
