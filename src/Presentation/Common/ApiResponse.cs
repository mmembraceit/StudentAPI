namespace StudentApi.Presentation.Common;

/// <summary>
/// Uniform HTTP response contract used across API success and failure responses.
/// </summary>
public record ApiResponse<T>(bool Success, T? Data, IReadOnlyList<string> Errors)
{
    /// <summary>
    /// Creates a successful API response payload.
    /// </summary>
    /// <param name="data">Response data payload.</param>
    /// <returns>A successful response object.</returns>
    public static ApiResponse<T> SuccessResponse(T? data)
    {
        return new ApiResponse<T>(true, data, Array.Empty<string>());
    }

    /// <summary>
    /// Creates a failed API response payload.
    /// </summary>
    /// <param name="errors">Error messages associated with the failure.</param>
    /// <returns>A failed response object.</returns>
    public static ApiResponse<T> FailureResponse(IEnumerable<string> errors)
    {
        return new ApiResponse<T>(false, default, errors.Distinct().ToArray());
    }
}