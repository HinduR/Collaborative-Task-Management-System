using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Grpc.Contracts;
using Shared.Grpc.Infrastructure;
using Shared.Grpc.Proto.Identity;
using Shared.Grpc.Proto.Metadata;
using Shared.Grpc.Util;

namespace Shared.Grpc.Extension;

public static class GrpcServiceExtension
{
    public static IServiceCollection AddSharedGrpc(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        string identityUrl = configuration["GrpcHostUrl:Identity"]
            ?? throw new InvalidOperationException(
                "GrpcHostUrl:Identity is not configured.");

        string metadataUrl = configuration["GrpcHostUrl:Metadata"]
            ?? throw new InvalidOperationException(
                "GrpcHostUrl:Metadata is not configured.");

        RegisterGrpcClient<
            IdentityUserService.IdentityUserServiceClient>(
            services,
            identityUrl);

        RegisterGrpcClient<
            MetadataLookupService.MetadataLookupServiceClient>(
            services,
            metadataUrl);

        services.AddScoped<IGrpcClientManager, GrpcClientManager>();
        services.AddScoped<IIdentityGrpcHelperService, IdentityGrpcHelperService>();
        services.AddScoped<IMetadataGrpcHelperService, MetadataGrpcHelperService>();

        return services;
    }

    private static IHttpClientBuilder RegisterGrpcClient<TClient>(
        IServiceCollection services,
        string url)
        where TClient : class
    {
        return services
            .AddGrpcClient<TClient>(options =>
            {
                options.Address = new Uri(url);
            })
            .ConfigurePrimaryHttpMessageHandler(() =>
                new SocketsHttpHandler
                {
                    EnableMultipleHttp2Connections = true,
                    PooledConnectionLifetime = TimeSpan.FromMinutes(5)
                });
    }
}