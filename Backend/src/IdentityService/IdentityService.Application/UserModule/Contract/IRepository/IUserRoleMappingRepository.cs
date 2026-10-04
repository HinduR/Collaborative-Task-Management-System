using IdentityService.Domain.Models;
using Shared.Common.contracts;

namespace IdentityService.Application.UserModule.Contract.IRepository;
/// <summary>
/// Represents the IUserRoleMappingRepository component.
/// </summary>
public interface IUserRoleMappingRepository : IRepositoryBase<UserRoleMapping>
{
}
