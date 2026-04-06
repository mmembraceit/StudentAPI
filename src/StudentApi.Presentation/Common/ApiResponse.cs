namespace StudentApi.Presentation.Common;


/// Uniform HTTP response contract for the API.
/// Used from controllers, middleware, and filters to return success and error with the same structure.
public record ApiResponse<T>(bool Success, T? Data, IReadOnlyList<string> Errors)
{
 
    /// Creates a standard success response.
    public static ApiResponse<T> SuccessResponse(T? data)
    {
        return new ApiResponse<T>(true, data, Array.Empty<string>());
    }

  
    /// Creates a standard failure response.
    public static ApiResponse<T> FailureResponse(IEnumerable<string> errors)
    {
        return new ApiResponse<T>(false, default, errors.Distinct().ToArray());
    }
}