using Microsoft.AspNetCore.Http;

namespace Shared.Exceptions.Infrastructure;

/// <summary>
/// Custom exception to be thrown when a conflict occurs.
/// </summary>
[Serializable]
public sealed class ConflictCustomException : BaseException
{
    /// <summary>
    /// Constructor for ConflictCustomException class.
    /// </summary>
    /// <param name="message">The custom message.</param>
    /// <param name="description">The custom description.</param>
    public ConflictCustomException(string message, string description)
        : base(message, description, StatusCodes.Status409Conflict) { }
}
