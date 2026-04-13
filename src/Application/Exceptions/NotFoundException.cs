namespace StudentApi.Application.Common.Exceptions;


/// When a requested business resource cannot be found.
public sealed class NotFoundException : Exception
{
  
    /// Creates a new not-found exception.
    public NotFoundException(string message)
        : base(message)
    {
    }
}