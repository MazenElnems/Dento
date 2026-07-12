using Dento.Enums;

namespace Dento.DTOs;

public class ConfirmCashPaymentResponseDto
{
    public string PaymentId { get; set; } = default!;
    public PaymentStatus Status { get; set; }
    public DateTime ConfirmedAt { get; set; }
}
