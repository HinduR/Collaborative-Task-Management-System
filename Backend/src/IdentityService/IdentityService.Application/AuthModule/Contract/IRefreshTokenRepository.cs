using IdentityService.Domain.Models;
using Shared.Common.contracts;
using Shared.Common.Repository;

namespace IdentityService.Application.AuthenticationModule.Contract.IRepository;

/// <summary>
/// Represents the IRefreshTokenRepository component.
/// </summary>
public interface IRefreshTokenRepository
    : IRepositoryBase<RefreshToken>
{
}
