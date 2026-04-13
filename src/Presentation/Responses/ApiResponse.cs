namespace StudentApi.Presentation.Common;


/// Uniform HTTP response contract used across API success and failure responses.
public record ApiResponse<T>(bool Success, T? Data, IReadOnlyList<string> Errors)
{
  
    /// Creates a successful API response payload.
    /// <returns>A successful response object.</returns>
    public static ApiResponse<T> SuccessResponse(T? data)
    {
        return new ApiResponse<T>(true, data, Array.Empty<string>());
    }

 
    /// Creates a failed API response payload.
    /// <returns>A failed response object.</returns>
    public static ApiResponse<T> FailureResponse(IEnumerable<string> errors)
    {
        return new ApiResponse<T>(false, default, errors.Distinct().ToArray());
    }
}