namespace StudentApi.Application.Common.Exceptions;


/// Application exception for missing business resources.
/// It is thrown by the Application layer and translated to HTTP 404 by <c>GlobalExceptionMiddleware</c> in Presentation.

public sealed class NotFoundException : Exception
{
    public NotFoundException(string message)
        : base(message)
    {
    }
}