using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Shared.Exceptions.Infrastructure;
using Shared.Logging.Infrastructure;
namespace IdentityService.API.Extension;

public static class ServiceExtension
{

    public static void ConfigureLoggerService(
        this WebApplicationBuilder builder,
        IServiceCollection services
    )
    {
        services.AddSharedLogging(
            builder.Configuration,
            "IdentityService");
    }

    public static void ConfigureExceptionHandler(this IServiceCollection services)
    {
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();
    }

}