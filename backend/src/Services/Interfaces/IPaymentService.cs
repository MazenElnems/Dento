using Dento.DTOs;
using Dento.Enums;
using Dento.Services.Interfaces;

namespace Dento.Services.Interfaces;

public interface IPaymentService
{
    /// <summary>
    /// Creates a payment for an appointment using the specified payment type.
    /// Delegates to the appropriate <see cref="IPaymentStrategy"/>.
    /// </summary>
    Task<PaymentResult> CreatePaymentAsync(string appointmentId, string idempotencyKey, string patientId, PaymentType paymentType);

    /// <summary>
    /// Confirms a cash payment. Only valid for payments with PaymentMethod = Cash and Status = Pending.
    /// </summary>
    Task<ConfirmCashPaymentResponseDto> ConfirmCashPaymentAsync(string paymentId, string receptionistId);
}
