namespace Shared.Exceptions.Infrastructure;

public abstract class BaseException : Exception
{
    public int StatusCode { get; }

    public string Description { get; }

    protected BaseException(
        string message,
        string description,
        int statusCode)
        : base(message)
    {
        Description = description;
        StatusCode = statusCode;
    }
}