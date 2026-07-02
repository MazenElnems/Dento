using Dento.Controllers.Common;
using Dento.Exceptions;

namespace Dento.Middlewares
{
    public class ExceptionMiddleware(RequestDelegate next)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                var baseException = ex as BaseException ??
                    new BaseException(
                        StatusCodes.Status500InternalServerError,
                        ex.Message);

                context.Response.StatusCode = baseException.StatusCode;
                context.Response.ContentType = "application/json";

                var response = ApiResponse<object>.ErrorResponse(
                    error: baseException.ErrorMessage ?? "An unexpected error occurred.",
                    statusCode: baseException.StatusCode,
                    message: "Request failed");

                await context.Response.WriteAsJsonAsync(response);
            }
        }
    }
}
