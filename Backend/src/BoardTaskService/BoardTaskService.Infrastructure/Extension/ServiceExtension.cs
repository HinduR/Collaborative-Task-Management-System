using BoardTaskService.Application.BoardModule.Contract.IService;
using BoardTaskService.Application.BoardModule.Service;
using BoardTaskService.Application.ProjectModule.Contract.IService;
using BoardTaskService.Application.ProjectModule.Service;
using BoardTaskService.Application.TaskModule.Contract.IHelperService;
using BoardTaskService.Application.TaskModule.Contract.IService;
using BoardTaskService.Application.TaskModule.HelperService;
using BoardTaskService.Application.TaskModule.Service;
using BoardTaskService.Infrastructure.BoardModule.Service;
using BoardTaskService.Infrastructure.Common;
using BoardTaskService.Infrastructure.Persistence.ApplicationContext;
using BoardTaskService.Infrastructure.Persistence.SeedData;
using BoardTaskService.Infrastructure.Realtime;
using BoardTaskService.Infrastructure.TaskModule.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Common.contracts;
using Shared.Common.Service;

namespace BoardTaskService.Infrastructure.Extension;

public static class ServiceExtension
{

    public static void ConfigureRequiredServices(
    this IServiceCollection services,
    IConfiguration configuration)
    {
        services.AddSingleton(configuration);
        ConfigureService(services);
        ConfigureRepository(services);
    }

    public static void ConfigureService(this IServiceCollection services)
    {
        services.AddScoped<IUserContext, UserContext>();
        services.AddScoped<IBoardService, BoardService>();
        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<IBoardAccessService, BoardAccessService>();
        services.AddScoped<IWorkflowColumnService, WorkflowColumnService>();
        services.AddScoped<ITaskService, TaskService>();
        services.AddScoped<IBoardTaskListService, BoardTaskListService>();
        services.AddScoped<ITaskCommentHelperService, TaskCommentHelperService>();
        services.AddScoped<ICommentService, CommentService>();
        services.AddScoped<IWorkflowColumnHelperService, WorkflowColumnHelperService>();
        services.AddScoped<ITaskQueryService, TaskQueryService>();
        services.AddScoped<ITaskMovementDataService, TaskMovementDataService>();
        services.AddScoped<ITaskMovementService, TaskMovementService>();
        services.AddScoped<IBoardRealtimeService, BoardRealtimeService>();
        services.AddSingleton<IBoardPresenceTracker, BoardPresenceTracker>();
    }

    public static void ConfigureRepository(this IServiceCollection services)
    {
        services.AddScoped<IRepoWrapper, RepoWrapper>();
    }

    public static void InitializeSeedData(this IServiceProvider services)
    {

        using (IServiceScope scope = services.CreateScope())
        {
            SeedData.Initialize(scope.ServiceProvider);
        }
    }

    public static void ConfigureDBContext(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<BoardTaskDbContext>(options =>
    {
        options.UseNpgsql(
            config.GetConnectionString("DefaultConnection"),
            npgsql =>
            {
                npgsql.MigrationsAssembly(
                    "BoardTaskService.Infrastructure");

                npgsql.MigrationsHistoryTable(
                    "__EFMigrationsHistory",
                    "boardtask");
            });
    });
    }

}
