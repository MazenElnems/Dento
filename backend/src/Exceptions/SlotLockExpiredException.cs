namespace Dento.Exceptions;

/// <summary>
/// Thrown when the patient tries to pay for an appointment whose slot lock has already expired.
/// </summary>
public class SlotLockExpiredException : BaseException
{
    public SlotLockExpiredException(string? errorMessage = null)
        : base(StatusCodes.Status409Conflict, errorMessage)
    {
    }
}
