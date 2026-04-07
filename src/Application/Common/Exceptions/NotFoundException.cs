namespace StudentApi.Application.Common.Exceptions;

/// <summary>
/// Exception raised when a requested business resource cannot be found.
/// </summary>
public sealed class NotFoundException : Exception
{
    /// <summary>
    /// Creates a new not-found exception.
    /// </summary>
    /// <param name="message">Human-readable error description.</param>
    public NotFoundException(string message)
        : base(message)
    {
    }
}