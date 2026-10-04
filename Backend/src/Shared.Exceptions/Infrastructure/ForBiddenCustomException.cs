using Microsoft.AspNetCore.Http;

namespace Shared.Exceptions.Infrastructure;

/// <summary>
/// Custom exception to be thrown when a forbidden occurs.
/// </summary>
[Serializable]
public sealed class ForBiddenCustomException : BaseException
{
    /// <summary>
    /// Constructor for ForBiddenCustomException class.
    /// </summary>
    /// <param name="message">The custom message.</param>
    /// <param name="description">The custom description.</param>
    public ForBiddenCustomException(string message, string description)
        : base(message, description, StatusCodes.Status403Forbidden) { }
}
