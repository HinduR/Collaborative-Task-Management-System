using IdentityService.Domain.Models;
using Shared.Common.contracts;

namespace IdentityService.Application.UserModule.Contract.IRepository;

/// <summary>
/// Represents the IFeatureRepository component.
/// </summary>
public interface IFeatureRepository : IRepositoryBase<Feature>
{
}
