using Microsoft.AspNetCore.Http;

namespace Shared.Exceptions.Infrastructure;

/// <summary>
/// Custom exception to be thrown when a not found occurs.
/// </summary>
[Serializable]
public sealed class NotFoundCustomException : BaseException
{
    /// <summary>
    /// Constructor for NotFoundCustomException class.
    /// </summary>
    /// <param name="message">The custom message.</param>
    /// <param name="description">The custom description.</param>
    public NotFoundCustomException(string message, string description)
        : base(message, description, StatusCodes.Status404NotFound) { }
}
