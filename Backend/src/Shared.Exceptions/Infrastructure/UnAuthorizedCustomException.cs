using Microsoft.AspNetCore.Http;

namespace Shared.Exceptions.Infrastructure;

/// <summary>
/// Custom exception to be thrown when an unauthorized occurs.
/// </summary>
[Serializable]
public sealed class UnAuthorizedCustomException : BaseException
{
    /// <summary>
    /// Constructor for UnAuthorizedCustomException class.
    /// </summary>
    /// <param name="message">The custom message.</param>
    /// <param name="description">The custom description.</param>
    public UnAuthorizedCustomException(string message, string description)
        : base(message, description, StatusCodes.Status401Unauthorized) { }
}
