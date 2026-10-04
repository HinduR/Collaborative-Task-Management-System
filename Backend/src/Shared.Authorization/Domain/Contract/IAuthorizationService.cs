namespace Shared.Authorisation.Domain.Contract;
/// <summary>
/// Represents the IAuthorizationService component.
/// </summary>
public interface IAuthorizationService
{
    bool HasPermission(string apiKey);
}
