using Dento.Constants;
using Dento.Controllers.Common;
using Dento.Exceptions;

namespace Dento.Middlewares;


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
            var response = ex switch
            {
                ResourceNotFoundException e => ApiResponse.ErrorResponse(
                    errorCode: ErrorCodes.ResourceNotFound,
                    statusCode: e.StatusCode,
                    message: e.Message),

                ConflictPaymentException e => ApiResponse.ErrorResponse(
                    errorCode: ErrorCodes.PaymentConflict,
                    statusCode: e.StatusCode,
                    message: e.Message),

                AppointmentPaymentException e => ApiResponse.ErrorResponse(
                    errorCode: ErrorCodes.AppointmentPaymentFailed,
                    statusCode: e.StatusCode,
                    message: e.Message),

                PaymentGatewayException e => ApiResponse.ErrorResponse(
                    errorCode: ErrorCodes.PaymentGatewayError,
                    statusCode: e.StatusCode,
                    message: e.Message),

                BaseException e => ApiResponse.ErrorResponse(
                    errorCode: ErrorCodes.UnhandledException,
                    statusCode: e.StatusCode,
                    message: e.Message),

                _ => ApiResponse.ErrorResponse(
                    errorCode: ErrorCodes.UnhandledException,
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred.")
            };

            context.Response.StatusCode = response.StatusCode;
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsJsonAsync(response);
        }
    }
}