using Dento.Constants;
using Dento.Data;
using Dento.DTOs;
using Dento.Enums;
using Dento.Exceptions;
using Dento.Models;
using Dento.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Dento.Services.Implementation;

/// <summary>
/// Orchestrates payment processing by delegating to the correct <see cref="IPaymentStrategy"/>
/// and handles cash payment confirmation.
/// </summary>
public class PaymentService : IPaymentService
{
    private readonly PaymentStrategyFactory _strategyFactory;
    private readonly AppDbContext _context;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(
        PaymentStrategyFactory strategyFactory,
        AppDbContext context,
        ILogger<PaymentService> logger)
    {
        _strategyFactory = strategyFactory;
        _context = context;
        _logger = logger;
    }

    public async Task<PaymentResult> CreatePaymentAsync(string appointmentId, string idempotencyKey, string patientId, PaymentType paymentType)
    {
        _logger.LogInformation(
            "Creating payment | AppointmentId: {AppointmentId} | PaymentType: {PaymentType} | PatientId: {PatientId}",
            appointmentId, paymentType, patientId);

        var strategy = _strategyFactory.Resolve(paymentType);
        return await strategy.ProcessAsync(appointmentId, patientId, idempotencyKey);
    }

    public async Task<ConfirmCashPaymentResponseDto> ConfirmCashPaymentAsync(string paymentId, string receptionistId)
    {
        _logger.LogInformation("Cash payment confirmation attempt | PaymentId: {PaymentId} | ReceptionistId: {ReceptionistId}",
            paymentId, receptionistId);

        var payment = await _context.Payments
            .FirstOrDefaultAsync(p => p.Id == paymentId);

        if (payment == null)
        {
            _logger.LogWarning("Cash payment confirmation failed — payment not found | PaymentId: {PaymentId}", paymentId);
            throw new BaseException(StatusCodes.Status404NotFound, "Payment not found.");
        }

        if (payment.PaymentMethod != PaymentMethod.Cash)
        {
            _logger.LogWarning("Cash payment confirmation failed — not a cash payment | PaymentId: {PaymentId} | Method: {Method}",
                paymentId, payment.PaymentMethod);
            throw new BaseException(StatusCodes.Status400BadRequest, "This payment is not a cash payment.");
        }

        if (payment.Status == PaymentStatus.Paid)
        {
            _logger.LogWarning("Cash payment confirmation failed — already confirmed | PaymentId: {PaymentId}", paymentId);
            throw new BaseException(StatusCodes.Status400BadRequest, "This payment has already been confirmed.");
        }

        if (payment.Status != PaymentStatus.Pending)
        {
            _logger.LogWarning("Cash payment confirmation failed — invalid status | PaymentId: {PaymentId} | Status: {Status}",
                paymentId, payment.Status);
            throw new BaseException(StatusCodes.Status400BadRequest, $"Cannot confirm a payment with status '{payment.Status}'.");
        }

        payment.Status    = PaymentStatus.Paid;
        payment.UpdatedAt = DateTime.UtcNow;

        payment.Events.Add(new PaymentEvent
        {
            Type       = PaymentEventType.CashPaymentConfirmed,
            CreatedAt  = DateTime.UtcNow,
            RawPayload = $"{{\"confirmedBy\":\"{receptionistId}\"}}"
        });

        await _context.SaveChangesAsync();

        _logger.LogInformation("Cash payment confirmed | PaymentId: {PaymentId} | ReceptionistId: {ReceptionistId}",
            paymentId, receptionistId);

        return new ConfirmCashPaymentResponseDto
        {
            PaymentId   = payment.Id,
            Status      = payment.Status,
            ConfirmedAt = payment.UpdatedAt!.Value
        };
    }
}
