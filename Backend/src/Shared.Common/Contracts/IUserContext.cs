namespace Shared.Common.contracts;

public interface IUserContext
{
  Guid GetUserId();
  void SetUserId(Guid userId);
  void SetRoleId(Guid roleId);
  Guid GetRoleId();
  void SetRoleName(string roleName);

  string GetRoleName();
  void SetUserName(string userName);
  string GetUserName();
  void SetToken(string token);
  string GetToken();

  void SetPermissions(List<string> permissions);
  List<string> GetPermissions();
}
