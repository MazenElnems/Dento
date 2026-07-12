using Dento.Enums;

namespace Dento.DTOs;

public class BookAppointmentRequestDto
{
    public required string SlotId { get; init; }
    public PaymentType PaymentType { get; init; } = PaymentType.Online;
}
