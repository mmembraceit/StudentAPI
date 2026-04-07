using FluentValidation;
using StudentApi.Application.Common.Exceptions;
using StudentApi.Presentation.Common;

namespace StudentApi.Presentation.Middleware;

/// <summary>
/// Middleware that converts unhandled exceptions into consistent API error responses.
/// </summary>
public sealed class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// Executes the next middleware component and handles unhandled exceptions.
    /// </summary>
    /// <param name="context">Current HTTP context.</param>
    /// <returns>A task that represents middleware execution.</returns>
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

    /// <summary>
    /// Maps a thrown exception to status code and response body, then writes the API error contract.
    /// </summary>
    /// <param name="context">Current HTTP context.</param>
    /// <param name="exception">Unhandled exception captured from downstream middleware.</param>
    /// <returns>A task that completes when the response has been written.</returns>
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