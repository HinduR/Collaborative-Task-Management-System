using ApiGateway.YARP.Service;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.RateLimiting;
using Shared.Logging.Infrastructure;

namespace ApiGateway.Extension;

public static class ServiceExtension
{
    public static void ConfigureLoggerService(this WebApplicationBuilder builder, IServiceCollection services)
    {
        services.AddSharedLogging(builder.Configuration, "ApiGateway");
    }

    public static IServiceCollection AddGatewayServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = ApiTokenHandler.SchemeName;

            options.DefaultChallengeScheme = ApiTokenHandler.SchemeName;
        })
        .AddScheme<AuthenticationSchemeOptions, ApiTokenHandler>(
            ApiTokenHandler.SchemeName,
            _ => { });

        services.AddAuthorization(options =>
        {
            options.AddPolicy("ApiTokenPolicy", policy =>
            {
                policy.AddAuthenticationSchemes(
                    ApiTokenHandler.SchemeName);

                policy.RequireAuthenticatedUser();
            });
        });

        int permitLimit = configuration.GetValue(
            "GatewaySettings:RateLimiting:PermitLimit",
            100);

        int windowSeconds = configuration.GetValue(
            "GatewaySettings:RateLimiting:WindowSeconds",
            60);

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode =
                StatusCodes.Status429TooManyRequests;

            options.AddFixedWindowLimiter(
                "GatewayRateLimit",
                limiterOptions =>
                {
                    limiterOptions.PermitLimit = permitLimit;
                    limiterOptions.Window = TimeSpan.FromSeconds(
                        windowSeconds);
                    limiterOptions.QueueLimit = 0;
                    limiterOptions.AutoReplenishment = true;
                });
        });

        int timeoutSeconds = configuration.GetValue(
            "GatewaySettings:RequestTimeout:DefaultSeconds",
            30);

        services.AddRequestTimeouts(options =>
        {
            options.AddPolicy(
                "GatewayTimeout",
                TimeSpan.FromSeconds(timeoutSeconds));
        });

        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.WithOrigins("http://localhost:4200")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });

        services.AddProblemDetails();

        return services;
    }
}