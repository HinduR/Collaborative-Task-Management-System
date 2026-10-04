using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Authorisation.Application.Service;
using Shared.Authorisation.Domain.Contract;
using Shared.Common.contracts;
using Shared.Common.Service;

namespace Shared.Authorisation.Infrastructure.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection
        ConfigureAuthorization(this IServiceCollection services)
    {

        services.AddScoped<IUserContext, UserContext>();
        services.AddScoped<IAuthorizationService, AuthorizationService>();
        
        return services;
    }
}