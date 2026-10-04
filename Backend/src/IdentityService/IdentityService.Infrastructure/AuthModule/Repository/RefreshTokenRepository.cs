using IdentityService.Application.AuthenticationModule.Contract.IRepository;
using IdentityService.Domain.Models;
using IdentityService.Infrastructure.Persistence.ApplicationContext;
using Shared.Common.Repository;

namespace IdentityService.Infrastructure.AuthenticationModule.Repository;

/// <summary>
/// Represents the RefreshTokenRepository component.
/// </summary>
public class RefreshTokenRepository
    : RepositoryBase<RefreshToken, IdentityDbContext>,
      IRefreshTokenRepository
{
    public RefreshTokenRepository(
        IdentityDbContext repositoryContext)
        : base(repositoryContext)
    {
    }
}
