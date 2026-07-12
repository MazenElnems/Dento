using Dento.Enums;

namespace Dento.Services.Interfaces;

/// <summary>
/// Defines a strategy for processing a payment for an appointment.
/// </summary>
public interface IPaymentStrategy
{
    /// <summary>
    /// The payment type this strategy handles.
    /// </summary>
    PaymentType SupportedPaymentType { get; }

    /// <summary>
    /// Processes the payment for the given appointment.
    /// </summary>
    Task<PaymentResult> ProcessAsync(string appointmentId, string patientId, string idempotencyKey);
}

/// <summary>
/// Encapsulates the result of a payment processing operation.
/// </summary>
public class PaymentResult
{
    public string PaymentId { get; set; } = default!;
    public PaymentStatus Status { get; set; }
    public PaymentMethod PaymentMethod { get; set; }

    /// <summary>
    /// For online payments: the client secret used to complete checkout.
    /// Null for cash payments.
    /// </summary>
    public string? ClientSecret { get; set; }

    /// <summary>
    /// For online payments: the Paymob public key.
    /// Null for cash payments.
    /// </summary>
    public string? PublicKey { get; set; }
}
