using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Shared.Authorisation.Domain.Contract;
using Shared.Exceptions.Infrastructure;


namespace Shared.Authorisation.Domain.Attribute;

/// <summary>
/// Represents attribute for the Custom Authorization
/// </summary>
[System.AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class CustomAuthorizeAttribute : System.Attribute, IAsyncAuthorizationFilter
{
    /// <summary>
    /// Represents the key applied on the API endpoint when using the attribute.
    /// </summary>
    private readonly string _apiKey;

    /// <summary>
    /// Constructor to get the dependecies
    /// </summary>
    /// <param name="apiKey">key applied on the api end point when using the attribute</param>
    public CustomAuthorizeAttribute(string apiKey)
    {
        _apiKey = apiKey;
    }

    /// <summary>
    /// Contains the Authorisation mechanism
    /// </summary>
    /// <param name="context"> AuthorisationFilterContext </param>
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
         IAuthorizationService? authorizationService = context.HttpContext.RequestServices.GetRequiredService<IAuthorizationService>();
        bool HasApiPermission =  authorizationService.HasPermission(_apiKey);

        if (!HasApiPermission)
        {
            throw new ForBiddenCustomException("Forbidden", "Access Denied");
        }
    }
}