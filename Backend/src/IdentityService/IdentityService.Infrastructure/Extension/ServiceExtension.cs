using IdentityService.Application.AuthenticationModule.Contract.IRepository;
using IdentityService.Application.AuthenticationModule.Contract.IService;
using IdentityService.Application.AuthenticationModule.Service;
using IdentityService.Application.Common;
using IdentityService.Application.RoleModule.Contract.IService;
using IdentityService.Application.UserModule.Contract.IService;
using IdentityService.Infrastructure.AuthenticationModule.Repository;
using IdentityService.Infrastructure.AuthenticationModule.Service;
using IdentityService.Infrastructure.Common;
using IdentityService.Infrastructure.Persistence.ApplicationContext;
using IdentityService.Infrastructure.Persistence.SeedData;
using IdentityService.Infrastructure.RoleModule.Service;
using IdentityService.Infrastructure.UserModule.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Common.contracts;
using Shared.Common.Service;
using Shared.Cryptography.Application.Cryptography.Contract;

namespace IdentityService.Infrastructure.Extension;

public static class ServiceExtension
{
    public static void ConfigureRequiredServices(
        this IServiceCollection services)
    {
        services.AddScoped<IRepoWrapper, RepoWrapper>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ILoginService, LoginService>();
        services.AddMemoryCache();
        services.AddSingleton<IGoogleLoginCodeService, GoogleLoginCodeService>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IUserRoleService, UserRoleService>();
        services.AddScoped<JwtService>();

    }
    public static void ConfigureDBContext(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<IdentityDbContext>(options =>
    {
        options.UseNpgsql(
            config.GetConnectionString("DefaultConnection"),
            npgsql =>
            {
                npgsql.MigrationsAssembly(
                    "IdentityService.Infrastructure");

                npgsql.MigrationsHistoryTable(
                    "__EFMigrationsHistory",
                    "identity");
            });
    });
    }
    public static void InitializeSeedData(this IServiceProvider services)
    {

        using (var scope = services.CreateScope())
        {
            SeedData.Initialize(scope.ServiceProvider);
        }
    }
    public static void ConfigureCryptographyServices(this IServiceCollection services)
		{
		
			_ = services.AddSingleton<IAuthenticity, Authenticity>();
		}
}
