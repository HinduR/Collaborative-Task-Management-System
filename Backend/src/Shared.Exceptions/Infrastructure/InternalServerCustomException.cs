using System.Net;
using Microsoft.AspNetCore.Http;

namespace Shared.Exceptions.Infrastructure;

/// <summary>
/// Custom exception to be thrown when an internal server error occurs.
/// </summary>
[Serializable]
public sealed class InternalServerCustomException : BaseException
{
    /// <summary>
    /// Constructor for InternalServerCustomException class.
    /// </summary>
    /// <param name="message">The custom message.</param>
    /// <param name="description">The custom description.</param>
    public InternalServerCustomException(string message, string description)
        : base(message, description, StatusCodes.Status500InternalServerError) { }
}
