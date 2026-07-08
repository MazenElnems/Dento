namespace Dento.Controllers.Common;

public class ApiResponse
{
    public bool Success { get; }
    public string? ErrorCode { get; set; }
    public int StatusCode { get; }
    public string Message { get; }
    public object? Data { get; }

    private ApiResponse(bool success, int statusCode, string message, object? data = null, string? errorCode = null)
    {
        Success = success;
        StatusCode = statusCode;
        Message = message;
        Data = data;
        ErrorCode = errorCode;
    }

    public static ApiResponse SuccessResponse(object? data, string message = "Data retrieved successfully", int statusCode = 200)
    {
        return new ApiResponse(true, statusCode, message, data);
    }

    public static ApiResponse SuccessResponse(string message, int statusCode = 200)
    {
        return new ApiResponse(true, statusCode, message);
    }

    public static ApiResponse ErrorResponse(string? errorCode, int statusCode = 400, string message = "An error occurred")
    {
        return new ApiResponse(false, statusCode, message, null, errorCode);
    }
}
