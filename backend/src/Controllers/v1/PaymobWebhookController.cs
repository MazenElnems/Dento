using Dento.Controllers.Common;
using Dento.Data;
using Dento.DTOs;
using Dento.Enums;
using Dento.Models;
using Dento.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dento.Controllers.v1;

[Route("api/webhooks/paymob")]
[ApiController]
public class PaymobWebhookController : BaseApiController
{
    private readonly IPaymobHmacVarifier _paymobHmacVerifier;
    private readonly AppDbContext _context;
    private readonly ILogger<PaymobWebhookController> _logger;

    public PaymobWebhookController(IPaymobHmacVarifier paymobHmacVerifier, AppDbContext context, ILogger<PaymobWebhookController> logger)
    {
        _paymobHmacVerifier = paymobHmacVerifier;
        _context = context;
        _logger = logger;
    }

    [HttpPost]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> HandleWebhook([FromBody] PaymobWebhookRequest request, [FromQuery] string hmac)
    {
        _logger.LogInformation("Paymob webhook received | TransactionId: {TransactionId} | OrderId: {OrderId} | Success: {Success} | AmountCents: {AmountCents}",
            request.Id, request.Order.Id, request.Success, request.AmountCents);

        // Varify HMAC 
        var data = string.Concat(
            request.AmountCents,
            request.CreatedAt,
            request.Currency,
            request.ErrorOccured.ToString().ToLowerInvariant(),
            request.HasParentTransaction.ToString().ToLowerInvariant(),
            request.Id,
            request.IntegrationId,
            request.Is3DSecure.ToString().ToLowerInvariant(),
            request.IsAuth.ToString().ToLowerInvariant(),
            request.IsCapture.ToString().ToLowerInvariant(),
            request.IsRefunded.ToString().ToLowerInvariant(),
            request.IsStandalonePayment.ToString().ToLowerInvariant(),
            request.IsVoided.ToString().ToLowerInvariant(),
            request.Order.Id,
            request.Owner,
            request.Pending.ToString().ToLowerInvariant(),
            request.SourceData.Pan,
            request.SourceData.SubType,
            request.SourceData.Type,
            request.Success.ToString().ToLowerInvariant()
        );

        var isVerified = _paymobHmacVerifier.Verify(data, hmac);

        if (!isVerified)
        {
            _logger.LogWarning("Webhook HMAC verification failed | TransactionId: {TransactionId} | OrderId: {OrderId}",
                request.Id, request.Order.Id);
            return Unauthorized("Invalid Hmac");
        }

        // Update the Payment 

        var payment = await _context.Payments
            .FirstOrDefaultAsync(x => x.IntentionId == request.Order.Id);

        if (payment == null)
        {
            _logger.LogWarning("Webhook received for unknown payment | OrderId: {OrderId} | TransactionId: {TransactionId}",
                request.Order.Id, request.Id);
            return NotFound();
        }

        var appointment = await _context.Appointments
            .Include(x => x.Slot)
            .FirstOrDefaultAsync(x => x.PatientId == payment.Id);

        if(appointment == null)
        {
            _logger.LogWarning("Webhook received but appointment not found | PaymentId: {PaymentId} | OrderId: {OrderId}",
                payment.Id, request.Order.Id);
            return BadRequest();
        }
        
        using var reader = new StreamReader(Request.Body);
        var rawPayload = await reader.ReadToEndAsync();

        payment.TransactionId = request.Id;
        payment.Status = request.Success ? PaymentStatus.Paid : PaymentStatus.Failed;

        // Log Payment Event
        payment.Events.Add(new PaymentEvent
        {
            RawPayload = rawPayload,
            Type = request.Success ? PaymentEventType.PaymentSucceeded : PaymentEventType.PaymentFailed,
            CreatedAt = DateTime.UtcNow
        });


        if (request.Success)
        {
            appointment.Status = AppointmentStatus.Confirmed;
            appointment.Slot.Status = SlotStatus.Booked;
            appointment.ConfirmedAt = DateTime.UtcNow;

            _logger.LogInformation("Payment confirmed — appointment status updated | PaymentId: {PaymentId} | AppointmentId: {AppointmentId} | TransactionId: {TransactionId}",
                payment.Id, appointment.Id, request.Id);
        }
        else
        {
            _logger.LogWarning("Payment failed via webhook | PaymentId: {PaymentId} | TransactionId: {TransactionId} | OrderId: {OrderId}",
                payment.Id, request.Id, request.Order.Id);
        }
        
        await _context.SaveChangesAsync();
        return Ok();
    }
}
