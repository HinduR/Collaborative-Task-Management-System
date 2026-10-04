using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Shared.Logging.Contracts;
using Serilog;

namespace Shared.Logging.Infrastructure
{
    public static class LoggingServiceExtensions
    {
        /// <summary>
        /// Adds shared logging services to the IServiceCollection.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <param name="serviceName"></param>
        public static void AddSharedLogging(this IServiceCollection services, IConfiguration configuration, string serviceName, string? fileName = null)
        {
            services.AddSingleton<ILogger>(sp => SharedLoggerConfiguration.CreateLoggerInstance(configuration, serviceName, fileName));
            services.AddSingleton(typeof(ILoggerManager<>), typeof(LoggerManager<>));
        }

        /// <summary>
        /// Adds shared logging services to the IServiceCollection with a default service name of "Microservice".
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        public static void AddSharedLogging(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSharedLogging(configuration, "Microservice");
        }
    }
}

