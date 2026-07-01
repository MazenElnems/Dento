namespace Dento.Controllers.Common;

public class ApiResponse<T>
{
    public bool Success { get; }
    public string Error { get; }
    public int StatusCode { get; }
    public string Message { get; }
    public T? Data { get; }

    private ApiResponse(bool success, int statusCode, string message, T? data = default, string error = "")
    {
        Success = success;
        StatusCode = statusCode;
        Message = message;
        Data = data;
        Error = error;
    }

    public static ApiResponse<T> SuccessResponse(T data, string message = "Data retrieved successfully", int statusCode = 200)
    {
        return new ApiResponse<T>(true, statusCode, message, data);
    }

    public static ApiResponse<T> ErrorResponse(string error, int statusCode = 400, string message = "An error occurred")
    {
        return new ApiResponse<T>(false, statusCode, message, default, error);
    }
}
