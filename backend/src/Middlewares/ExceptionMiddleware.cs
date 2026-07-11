using Dento.Constants;
using Dento.Controllers.Common;
using Dento.Exceptions;

namespace Dento.Middlewares;

public class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (ResourceNotFoundException ex)
        {
            await HandleExceptionAsync(
                context,
                ex,
                ErrorCodes.ResourceNotFound,
                ex.StatusCode,
                ex.Message);
        }
        catch (ConflictPaymentException ex)
        {
            await HandleExceptionAsync(
                context,
                ex,
                ErrorCodes.PaymentConflict,
                ex.StatusCode,
                ex.Message);
        }
        catch (AppointmentPaymentException ex)
        {
            await HandleExceptionAsync(
                context,
                ex,
                ErrorCodes.AppointmentPaymentFailed,
                ex.StatusCode,
                ex.Message);
        }
        catch (SlotLockExpiredException ex)
        {
            await HandleExceptionAsync(
                context,
                ex,
                ErrorCodes.SlotLockExpired,
                ex.StatusCode,
                ex.Message);
        }
        catch (PaymentGatewayException ex)
        {
            await HandleExceptionAsync(
                context,
                ex,
                ErrorCodes.PaymentGatewayError,
                ex.StatusCode,
                ex.Message);
        }
        catch (BaseException ex)
        {
            await HandleExceptionAsync(
                context,
                ex,
                ErrorCodes.UnhandledException,
                ex.StatusCode,
                ex.Message);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(
                context,
                ex,
                ErrorCodes.UnhandledException,
                StatusCodes.Status500InternalServerError,
                "An unexpected error occurred.");
        }
    }

    private async Task HandleExceptionAsync(
        HttpContext context,
        Exception ex,
        string errorCode,
        int statusCode,
        string message)
    {
        var response = ApiResponse.ErrorResponse(
            errorCode: errorCode,
            statusCode: statusCode,
            message: message);

        if (ex is BaseException)
        {
            logger.LogWarning(
                ex,
                "Handled exception — {ExceptionType} | {RequestMethod} {RequestPath} | TraceId: {TraceId} | StatusCode: {StatusCode} | ErrorCode: {ErrorCode}",
                ex.GetType().Name,
                context.Request.Method,
                context.Request.Path,
                context.TraceIdentifier,
                response.StatusCode,
                response.ErrorCode);
        }
        else
        {
            logger.LogError(
                ex,
                "Unhandled exception | {RequestMethod} {RequestPath} | TraceId: {TraceId} | StatusCode: {StatusCode}",
                context.Request.Method,
                context.Request.Path,
                context.TraceIdentifier,
                response.StatusCode);
        }

        context.Response.StatusCode = response.StatusCode;
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsJsonAsync(response);
    }
}