using System.Net;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shared.Logging.Contracts;

namespace Shared.Exceptions.Infrastructure;

/// <summary>
/// Global exception handler.
/// </summary>
public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly IHostEnvironment _env;

    /// <summary>
    /// Constructor for GlobalExceptionHandler class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="env">The environment instance.</param>
    public GlobalExceptionHandler(IHostEnvironment env)
    {
        _env = env;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken
    )
    {
        ILoggerManager<GlobalExceptionHandler> _logger =
            httpContext.RequestServices.GetRequiredService<
                ILoggerManager<GlobalExceptionHandler>
            >();
        if (_logger != null)
        {
            _logger.LogError(
                $"System encountered an exception: {exception.Message}\nStackTrace: {exception.StackTrace}"
            );
        }

        ProblemDetails? problemDetails = new ProblemDetails();
        int statusCode = (int)HttpStatusCode.InternalServerError;

        if (exception is BaseException customException)
        {
            statusCode = customException.StatusCode;
            problemDetails.Status = statusCode;
            problemDetails.Title = customException.Description;
            problemDetails.Detail = customException.Message;
        }
        else
        {
            problemDetails.Status = (int)HttpStatusCode.InternalServerError;
            problemDetails.Title = "Internal Server Error";
            problemDetails.Detail = _env.IsDevelopment()
                ? exception.Message
                : "An error occurred. Please try again later.";
        }

        httpContext.Response.StatusCode = statusCode;

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
