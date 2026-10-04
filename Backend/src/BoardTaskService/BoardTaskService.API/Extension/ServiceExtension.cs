using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Cryptography;
using Shared.Exceptions.Infrastructure;
using Shared.Logging.Infrastructure;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using Shared.SignalR.Constants;
namespace BoardTaskService.API.Extension;

public static class ServiceExtension
{

    public static void ConfigureLoggerService(
        this WebApplicationBuilder builder,
        IServiceCollection services
    )
    {
        services.AddSharedLogging(builder.Configuration, "BoardTaskService");
    }

    public static void ConfigureExceptionHandler(this IServiceCollection services)
    {
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();
    }
    public static void ConfigureJwtAuthentication(
          this IServiceCollection services,
          IConfiguration configuration)
    {
        string publicKeyPem = configuration["JwtConfig:PublicKey"]
            ?? throw new InvalidOperationException(
                "JwtConfig:PublicKey is not configured.");

        RSA rsa = RSA.Create();
        rsa.ImportFromPem(publicKeyPem);

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        string? accessToken =
                            context.Request.Query[
                                SignalRConstants.AccessTokenQueryParameter];

                        PathString path =
                            context.HttpContext.Request.Path;

                        if (!string.IsNullOrWhiteSpace(accessToken) &&
                            path.StartsWithSegments(
                                SignalRConstants.BoardHubPath))
                        {
                            context.Token = accessToken;
                        }

                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        if (context.Principal?.Identity
                            is not ClaimsIdentity identity)
                        {
                            return Task.CompletedTask;
                        }

                        string? userId =
                            identity.FindFirst("sub")?.Value;

                        if (!string.IsNullOrWhiteSpace(userId) &&
                            !identity.HasClaim(
                                claim =>
                                    claim.Type ==
                                    ClaimTypes.NameIdentifier))
                        {
                            identity.AddClaim(
                                new Claim(
                                    ClaimTypes.NameIdentifier,
                                    userId));
                        }

                        return Task.CompletedTask;
                    }
                };

                options.TokenValidationParameters =
                    new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new RsaSecurityKey(rsa),

                        ValidateIssuer = true,
                        ValidIssuer = configuration["JwtConfig:Issuer"],

                        ValidateAudience = true,
                        ValidAudience = configuration["JwtConfig:Audience"],

                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero,

                        NameClaimType = ClaimTypes.Name,
                        RoleClaimType = ClaimTypes.Role
                    };
            });

        services.AddAuthorization();
    }
}