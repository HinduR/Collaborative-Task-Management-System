using Serilog;
using Shared.Logging.Contracts;


namespace Shared.Logging.Infrastructure;

/// <summary>
/// Class <c>LoggerManager</c> used to define the methods related for logger manager.
/// </summary>
/// <typeparam name="T"></typeparam>
public class LoggerManager<T> : ILoggerManager<T>
{
    private readonly ILogger _logger;

    public LoggerManager(ILogger logger)
    {
        _logger = logger.ForContext<T>();
    }

    /// <inheritdoc/>
    public void LogInformation(string message, params object[] args) => _logger.Information(message, args);

    /// <inheritdoc/>
    public void LogWarning(string message, params object[] args) => _logger.Warning(message, args);

    /// <inheritdoc/>
    public void LogError(string message, Exception? exception = null, params object[] args)
    {
        if (exception != null)
            _logger.Error(exception, message, args);
        else
            _logger.Error(message, args);
    }

    /// <inheritdoc/>
    public void LogDebug(string message, params object[] args) => _logger.Debug(message, args);


}
