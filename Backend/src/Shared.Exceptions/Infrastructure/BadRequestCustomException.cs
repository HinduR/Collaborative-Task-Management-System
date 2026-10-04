using Microsoft.AspNetCore.Http;

namespace Shared.Exceptions.Infrastructure;

/// <summary>
/// Exception to be thrown when a bad request occurs.
/// </summary>
[Serializable]
public sealed class BadRequestCustomException : BaseException
{
    /// <summary>
    /// Constructor for BadRequestCustomException class.
    /// </summary>
    /// <param name="message">The custom message.</param>
    /// <param name="description">The custom description.</param>
    public BadRequestCustomException(string message, string description)
        : base(message, description, StatusCodes.Status400BadRequest) { }
}
