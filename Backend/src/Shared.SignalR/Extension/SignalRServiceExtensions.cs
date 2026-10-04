using Microsoft.Extensions.DependencyInjection;

namespace Shared.SignalR.Extensions;

public static class SignalRServiceExtensions
{
    public static IServiceCollection AddSharedSignalR(
        this IServiceCollection services)
    {
        services.AddSignalR();

        return services;
    }
}