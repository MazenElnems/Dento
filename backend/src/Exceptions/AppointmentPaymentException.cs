namespace Dento.Exceptions;

public class AppointmentPaymentException : BaseException
{
    public AppointmentPaymentException(string? errorMessage = null)
        : base(StatusCodes.Status400BadRequest, errorMessage)
    {
    }
}
