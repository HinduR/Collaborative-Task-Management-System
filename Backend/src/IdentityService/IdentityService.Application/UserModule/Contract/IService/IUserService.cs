using IdentityService.Application.UserModule.Dto;

namespace IdentityService.Application.UserModule.Contract.IService;

/// <summary>
/// Represents the IUserService component.
/// </summary>
public interface IUserService
{
    Task<UserListResponseDto> GetUsers(
        string? search,
        CancellationToken cancellationToken);

    Task<AuthenticatedUserDto> GetUserById(
      Guid? userId,
      CancellationToken cancellationToken);

    Task<List<UserListItemDto>> GetUserListByIdAsync(
     List<Guid> userIdList,
     CancellationToken cancellationToken);
}
