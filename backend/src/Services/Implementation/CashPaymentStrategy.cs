using Dento.Data;
using Dento.Enums;
using Dento.Exceptions;
using Dento.Models;
using Dento.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Dento.Services.Implementation;

/// <summary>
/// Handles cash payments. Immediately confirms the appointment and creates a
/// pending payment record that must be confirmed by a Receptionist at the clinic.
/// </summary>
public class CashPaymentStrategy : IPaymentStrategy
{
    private readonly AppDbContext _context;
    private readonly ILogger<CashPaymentStrategy> _logger;

    public PaymentType SupportedPaymentType => PaymentType.Cash;

    public CashPaymentStrategy(AppDbContext context, ILogger<CashPaymentStrategy> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PaymentResult> ProcessAsync(string appointmentId, string patientId, string idempotencyKey)
    {
        _logger.LogInformation("Processing cash payment | AppointmentId: {AppointmentId} | PatientId: {PatientId}",
            appointmentId, patientId);

        var appointment = await _context.Appointments
            .Include(x => x.Dentist)
            .Include(x => x.Patient)
            .Include(x => x.Payment)
            .Include(x => x.Slot)
            .FirstOrDefaultAsync(x => x.Id == appointmentId);

        if (appointment == null)
            throw new ResourceNotFoundException(nameof(Appointment));

        if (appointment.PatientId != patientId)
        {
            _logger.LogWarning("Unauthorized cash payment attempt | AppointmentId: {AppointmentId} | PatientId: {PatientId}",
                appointmentId, patientId);
            throw new AppointmentPaymentException("You are not authorized to pay for this appointment.");
        }

        if (appointment.Payment != null && appointment.Payment.Status == PaymentStatus.Paid)
            throw new AppointmentPaymentException("Payment already completed.");

        if (appointment.Payment != null && appointment.Payment.Status == PaymentStatus.Pending)
            throw new AppointmentPaymentException("Payment already in-progress.");

        var payment = new Payment
        {
            IdempotencyKey = idempotencyKey,
            Amount         = appointment.Dentist.ConsultationFee ?? 200,
            Currency       = "EGP",
            PayerEmail     = appointment.Patient.Email,
            PayerName      = appointment.Patient.FullName,
            Status         = PaymentStatus.Pending,
            PaymentMethod  = PaymentMethod.Cash,
            CreatedAt      = DateTime.UtcNow,
        };

        // For cash: immediately confirm the appointment and book the slot
        appointment.PaymentId    = payment.Id;
        appointment.Status       = AppointmentStatus.Confirmed;
        appointment.ConfirmedAt  = DateTime.UtcNow;
        appointment.Slot.Status  = SlotStatus.Booked;
        appointment.Slot.LockedUntil = null;

        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Cash payment created — appointment confirmed immediately | PaymentId: {PaymentId} | AppointmentId: {AppointmentId}",
            payment.Id, appointmentId);

        return new PaymentResult
        {
            PaymentId     = payment.Id,
            Status        = payment.Status,
            PaymentMethod = PaymentMethod.Cash,
            ClientSecret  = null,
            PublicKey     = null
        };
    }
}
