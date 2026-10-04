using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;

namespace Shared.Logging.Infrastructure
{

    public static class SharedLoggerConfiguration
    {
        public static ILogger CreateLoggerInstance(IConfiguration configuration, string serviceName, string? overrideFileName = null)
        {
            string basePath = configuration["PathConfig:BaseLogPath"] ?? string.Empty;
            basePath = Path.Combine(basePath, "logs");

            string fileName = !string.IsNullOrEmpty(overrideFileName) ? overrideFileName : string.Concat(serviceName.ToLower(), "_");

            string filePath = Path.ChangeExtension(Path.Combine(basePath, serviceName, fileName), ".log");

            string minimumLevelText = configuration["Serilog:MinimumLevel:Default"] ?? "Information";

            LogEventLevel minimumLevel = Enum.TryParse<LogEventLevel>(minimumLevelText, true, out LogEventLevel parsedLevel)
                ? parsedLevel
                : LogEventLevel.Information;

            return new LoggerConfiguration()
      .MinimumLevel.Is(minimumLevel)
      .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
      .Enrich.FromLogContext()
      .Enrich.WithProperty("ServiceName", serviceName)
      .WriteTo.Console()
      .WriteTo.File(
          path: filePath,
          rollingInterval: RollingInterval.Day,
          retainedFileCountLimit: 14,
          fileSizeLimitBytes: 20 * 1024 * 1024,
          rollOnFileSizeLimit: true)
      .CreateLogger();
        }
    }
}