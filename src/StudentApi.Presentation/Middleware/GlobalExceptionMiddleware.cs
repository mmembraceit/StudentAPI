using FluentValidation;
using StudentApi.Application.Common.Exceptions;
using StudentApi.Presentation.Common;

namespace StudentApi.Presentation.Middleware;


/// Global middleware that transforms exceptions into consistent HTTP responses.
/// Centralizes error handling and logging. It is wired in <c>Program.cs</c> and consumes exceptions thrown by Application.

public sealed class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

   
    /// Executes the next component in the pipeline and captures any exception.
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }

  
    /// Translates the exception into an HTTP status code, output errors, and log level.
    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, errors, logLevel) = exception switch
        {
            NotFoundException notFoundException => (StatusCodes.Status404NotFound, new[] { notFoundException.Message }, LogLevel.Warning),
            ValidationException validationException => (StatusCodes.Status400BadRequest, validationException.Errors.Select(error => error.ErrorMessage).ToArray(), LogLevel.Warning),
            _ => (StatusCodes.Status500InternalServerError, new[] { "An unexpected error occurred." }, LogLevel.Error)
        };

        _logger.Log(logLevel, exception, "Request failed with status code {StatusCode}", statusCode);

        context.Response.StatusCode = statusCode;

        var response = ApiResponse<object?>.FailureResponse(errors);
        await context.Response.WriteAsJsonAsync(response);
    }
}