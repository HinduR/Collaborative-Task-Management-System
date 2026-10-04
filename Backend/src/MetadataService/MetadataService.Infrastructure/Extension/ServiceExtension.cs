using MetadataService.Application.Common;
using MetadataService.Application.Contract.IRepository;
using MetadataService.Application.Contract.IService;
using MetadataService.Infrastructure.Common;
using MetadataService.Infrastructure.Persistence.ApplicationContext;
using MetadataService.Infrastructure.Persistence.SeedData;
using MetadataService.Infrastructure.Repository;
using MetadataService.Infrastructure.Service;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Common.contracts;
using Shared.Common.Service;
using Shared.Exceptions.Infrastructure;
using Shared.Logging.Infrastructure;
using Shared.Redis.Contract;
using Shared.Redis.Service;
using StackExchange.Redis;

namespace MetadataService.Infrastructure.Extension;

public static class ServiceExtension
{

    public static void ConfigureRequiredServices(
    this IServiceCollection services)
    {
        ConfigureService(services);
        ConfigureRepository(services);
    }

    public static void ConfigureService(this IServiceCollection services)
    {
        services.AddScoped<IMetaDataService, MetaDataService>();
    }

    public static void ConfigureRepository(this IServiceCollection services)
    {
        services.AddScoped<IRefSetRepository, RefSetRepository>();
        services.AddScoped<ISetRefTermRepository, SetRefTermRepository>();
        services.AddScoped<IRefTermRepository, RefTermRepository>();
        services.AddScoped<IUserContext, UserContext>();
        services.AddScoped<IRepoWrapper, RepoWrapper>();
    }

    public static void ConfigureLoggerService(
        this WebApplicationBuilder builder,
        IServiceCollection services
    )
    {
        services.AddSharedLogging(builder.Configuration, "BoardTaskService");
    }
    public static void ConfigureRedis(
       this IServiceCollection services,
       IConfiguration configuration)
    {
        string connectionString =
            configuration.GetValue<string>("Redis")
            ?? throw new InvalidOperationException(
                "Redis configuration is missing.");

        services.AddSingleton<IConnectionMultiplexer>(
            _ => ConnectionMultiplexer.Connect(
                connectionString));

        services.AddSingleton<
            IRedisCacheService,
            RedisCacheService>();
    }

    public static void ConfigureExceptionHandler(this IServiceCollection services)
    {
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();
    }
    public static void ConfigureDBContext(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<MetadataDbContext>(options =>
    {
        options.UseNpgsql(
            config.GetConnectionString("DefaultConnection"),
            npgsql =>
            {
                npgsql.MigrationsAssembly(
                    "MetadataService.Infrastructure");

                npgsql.MigrationsHistoryTable(
                    "__EFMigrationsHistory",
                    "metadata");
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

}