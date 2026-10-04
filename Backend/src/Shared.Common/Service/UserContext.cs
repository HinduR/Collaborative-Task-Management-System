using Shared.Common.contracts;

namespace Shared.Common.Service;

/// <summary>
/// Represents the UserContext component.
/// </summary>
public class UserContext : IUserContext
{
    private Guid _userId = Guid.Empty;
    private Guid _roleId = Guid.Empty;
    private string _roleName = string.Empty;
    private string _userName = string.Empty;
    private string _token = string.Empty;
    private List<string> _permissions = [];

    public void SetUserId(Guid userId) => _userId = userId;

    public void SetRoleId(Guid roleId) => _roleId = roleId;

    public void SetRoleName(string roleName) => _roleName = roleName;

    public void SetUserName(string userName) => _userName = userName;


    public Guid GetUserId() => _userId;
    public Guid GetRoleId() => _roleId;

    public string GetRoleName() => _roleName;

    public string GetUserName() => _userName;



    public void SetToken(string token)
    {
        _token = token;
    }

    public string GetToken()
    {
        return _token;
    }

    public void SetPermissions(List<string> permissions)
    {
        _permissions = permissions;
    }

    public List<string> GetPermissions()
    {
        return _permissions;
             
    }
}
