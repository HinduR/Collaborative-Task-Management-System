using IdentityService.Application.UserModule.Dto;

namespace IdentityService.Application.UserModule.Contract.IService;

/// <summary>
/// Represents the IUserRoleService component.
/// </summary>
public interface IUserRoleService
{
    Task<UserRoleMappingResponseDto> AssignRoleAsync(
        Guid userId,
        Guid roleId,
        CancellationToken cancellationToken);
}
