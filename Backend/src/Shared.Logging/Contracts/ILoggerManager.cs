namespace Shared.Logging.Contracts;

/// <summary>
/// Interface <c>ILoggerManager</c> used to define the methods related for logger manager.
/// </summary>
/// <typeparam name="T">Type of the class using the logger.</typeparam>
public interface ILoggerManager<T>
{
    /// <summary>
    /// Log information to the log provider.
    /// </summary>
    /// <param name="message">The message to be logged.</param>
    void LogInformation(string message, params object[] args);

    /// <summary>
    /// Log warning to the log provider.
    /// </summary>
    /// <param name="message">The message to be logged.</param>
    void LogWarning(string message, params object[] args);

    /// <summary>
    /// Log error to the log provider.
    /// </summary>
    /// <param name="message">The message to be logged.</param>
    void LogError(string message, Exception? exception = null, params object[] args);

    /// <summary>
    /// Log debug information to the log provider.
    /// </summary>
    /// <param name="message">The message to be logged.</param>
    void LogDebug(string message, params object[] args);
}

