using IdentityService.Application.UserModule.Contract.IRepository;
using IdentityService.Domain.Models;
using IdentityService.Infrastructure.Persistence.ApplicationContext;
using Shared.Common.Repository;

namespace IdentityService.Infrastructure.UserModule.Repository;

/// <summary>Provides repository operations for <see cref="UserRoleMapping"/> entities.</summary>
/// <param name="repositoryContext">The Identity database context used by the repository.</param>
public class UserRoleMappingRepository(
    IdentityDbContext repositoryContext)
    : RepositoryBase<UserRoleMapping, IdentityDbContext>(repositoryContext),
      IUserRoleMappingRepository
{
}