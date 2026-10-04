using IdentityService.Domain.Models;
using Shared.Common.contracts;

namespace IdentityService.Application.UserModule.Contract.IRepository;

/// <summary>
/// Represents the IUserRepository component.
/// </summary>
public interface IUserRepository : IRepositoryBase<User>
{
}
