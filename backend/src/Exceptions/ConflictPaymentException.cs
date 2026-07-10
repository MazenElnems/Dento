namespace Dento.Exceptions;

public class ConflictPaymentException : BaseException
{
    public ConflictPaymentException(string? errorMessage = null) 
        : base(StatusCodes.Status409Conflict, errorMessage)
    {
    }
}
