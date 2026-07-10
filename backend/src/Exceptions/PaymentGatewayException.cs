namespace Dento.Exceptions;

public class PaymentGatewayException : BaseException
{
    public PaymentGatewayException(int statusCode, string? errorMessage = null)
        : base(statusCode, errorMessage)
    {
    }
}
