using Dento.Data;
using Dento.DTOs;
using Dento.Enums;
using Dento.Exceptions;
using Dento.Models;
using Dento.Options;
using Dento.Services.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Dento.Services.Implementation;

public class PaymentService : IPaymentService
{
    private readonly AppDbContext _context;
    private readonly PaymobSettings _paymob;
    private readonly IHttpClientFactory _factory;
    private readonly ClientSettings _client;
    private readonly ApplicationOptions _appOptions;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(IOptions<PaymobSettings> paymob, AppDbContext context, IHttpClientFactory factory, IOptions<ClientSettings> client, IOptions<ApplicationOptions> appOptions, ILogger<PaymentService> logger)
    {
        _paymob = paymob.Value;
        _context = context;
        _factory = factory;
        _client = client.Value;
        _appOptions = appOptions.Value;
        _logger = logger;
    }

    public async Task<string> CreatePaymentIntent(string appointmentId, string idempotencyKey, string patientId)
    {
        _logger.LogInformation("Creating payment intent | AppointmentId: {AppointmentId} | IdempotencyKey: {IdempotencyKey} | PatientId: {PatientId}",
            appointmentId, idempotencyKey, patientId);

        var appointment = await _context.Appointments
            .Include(x => x.Dentist)
            .Include(x => x.Patient)
            .Include(x => x.Payment)
            .Include(x => x.Slot)
            .FirstOrDefaultAsync(x => x.Id == appointmentId);

        if (appointment == null)
            throw new ResourceNotFoundException(nameof(Appointment));

        // Ownership check — only the patient who created the appointment can pay
        if (appointment.PatientId != patientId)
        {
            _logger.LogWarning("Unauthorized payment attempt | AppointmentId: {AppointmentId} | RequestingPatientId: {PatientId} | OwnerPatientId: {OwnerId}",
                appointmentId, patientId, appointment.PatientId);
            throw new AppointmentPaymentException("You are not authorized to pay for this appointment.");
        }

        // Slot lock check — verify the 10-minute window hasn't expired
        if (appointment.Slot.Status != SlotStatus.Locked ||
            appointment.Slot.LockedUntil == null ||
            appointment.Slot.LockedUntil <= DateTime.UtcNow)
        {
            _logger.LogWarning("Payment intent rejected — slot lock expired | AppointmentId: {AppointmentId} | SlotId: {SlotId} | LockedUntil: {LockedUntil}",
                appointmentId, appointment.SlotId, appointment.Slot.LockedUntil);
            throw new SlotLockExpiredException("The slot reservation has expired. Please select a new time slot.");
        }

        // Already Payment Check
        if (appointment.Payment != null && appointment.Payment.Status == PaymentStatus.Pending)
        {
            _logger.LogWarning("Payment already in-progress | AppointmentId: {AppointmentId} | PaymentId: {PaymentId}",
                appointmentId, appointment.Payment.Id);
            throw new AppointmentPaymentException("Payment already in-progress.");
        }

        if (appointment.Payment != null && appointment.Payment.Status == PaymentStatus.Paid)
        {
            _logger.LogWarning("Payment already completed | AppointmentId: {AppointmentId} | PaymentId: {PaymentId}",
                appointmentId, appointment.Payment.Id);
            throw new AppointmentPaymentException("Payment already completed.");
        }

        var payment = new Payment
        {
            IdempotencyKey = idempotencyKey,
            Amount         = appointment.Dentist.ConsultationFee ?? 200,
            Currency       = "EGP",
            PayerEmail     = appointment.Patient.Email,
            PayerName      = appointment.Patient.FullName,
            Status         = PaymentStatus.Pending,
            PaymentMethod  = PaymentMethod.Card,
            CreatedAt      = DateTime.UtcNow,
        };

        appointment.PaymentId = payment.Id;
        _context.Payments.Add(payment);

        // First insert the Pending Payment row to ensure idempotency
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlExp && (sqlExp.Number == 2601 || sqlExp.Number == 2627))
        {
            _logger.LogWarning(
                ex,
                "Duplicate idempotency key '{IdempotencyKey}' detected while creating payment.",
                payment.IdempotencyKey
            );

            throw new ConflictPaymentException("A payment with the same idempotency key already exists.");
        }

        var body = new
        {
            amount           = appointment.Dentist.ConsultationFee * 100,
            currency         = "EGP",
            payment_methods  = new[] { 5772602 },

            items = new[]
            {
                new
                {
                    name        = "Dental Consultation",
                    amount      = appointment.Dentist.ConsultationFee * 100,
                    description = $"Consultation with Dr. {appointment.Dentist.FullName}",
                    quantity    = 1
                }
            },

            billing_data = new
            {
                first_name   = appointment.Patient.FirstName,
                last_name    = appointment.Patient.LastName,
                email        = appointment.Patient.Email,
                phone_number = appointment.Patient.PhoneNumber
            },

            special_reference = idempotencyKey,

            notification_url  = $"{_appOptions.ApiBaseUrl}/{_paymob.WebhookEndpointUrl}",
            redirection_url   = $"{_client.Host}/payment/result"
        };

        using var client = _factory.CreateClient();

        var url = $"{_paymob.BaseUrl}/{_paymob.CreatePaymentIntentPath}";

        _logger.LogInformation("Calling Paymob API | PaymentId: {PaymentId} | Url: {Url}", payment.Id, url);

        _logger.LogInformation(
            "Paymob URLs | NotificationUrl: {NotificationUrl} | RedirectionUrl: {RedirectionUrl}",
            body.notification_url,
            body.redirection_url
        );

        var request = new HttpRequestMessage(HttpMethod.Post, url);

        request.Headers.Authorization = new AuthenticationHeaderValue("Token", _paymob.SecretKey);

        request.Content = JsonContent.Create(body);

        HttpResponseMessage response;
        try
        {
            response = await client.SendAsync(request);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or TimeoutException)
        {
            _logger.LogError(ex, "Network failure calling Paymob for payment {PaymentId}", payment.Id);

            payment.Status      = PaymentStatus.Failed;
            payment.FauilerReason = "Network error contacting payment gateway";
            payment.UpdatedAt   = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            throw new PaymentGatewayException(StatusCodes.Status502BadGateway, "Unable to reach payment gateway. Please try again.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unknown exception thrown for payment {PaymentId}", payment.Id);

            payment.Status      = PaymentStatus.Failed;
            payment.FauilerReason = "Unknown error contacting payment gateway";
            payment.UpdatedAt   = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            throw;
        }

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync();

            _logger.LogError(
                "Failed to create payment intent. Status: {StatusCode}, Body: {Body}",
                response.StatusCode,
                errorBody
            );

            payment.Status      = PaymentStatus.Failed;
            payment.FauilerReason = "Failed to create payment intent";
            payment.UpdatedAt   = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            throw new PaymentGatewayException(StatusCodes.Status502BadGateway, "Unable to create payment. Please try again.");
        }

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy        = JsonNamingPolicy.SnakeCaseLower
        };

        var rawContent = await response.Content.ReadAsStringAsync();

        CreatePaymentIntentPaymobResponseDto? paymentIntentResponse;
        try
        {
            paymentIntentResponse = JsonSerializer.Deserialize<CreatePaymentIntentPaymobResponseDto>(rawContent, options);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse Paymob response for payment {PaymentId}. Raw: {Raw}", payment.Id, rawContent);
            paymentIntentResponse = null;
        }

        if (paymentIntentResponse is null)
        {
            payment.Status      = PaymentStatus.Failed;
            payment.FauilerReason = "Unable to parse payment gateway response";
            payment.UpdatedAt   = DateTime.UtcNow;

            payment.Events.Add(new PaymentEvent
            {
                CreatedAt  = DateTime.UtcNow,
                RawPayload = rawContent,
                Type       = PaymentEventType.PaymentIntentCreated
            });

            await _context.SaveChangesAsync();

            throw new PaymentGatewayException(StatusCodes.Status502BadGateway, "Unable to create payment. Please try again.");
        }

        payment.IntentionId  = paymentIntentResponse.IntentionOrderId;
        payment.ClientSecret = paymentIntentResponse.ClientSecret;
        payment.Status       = PaymentStatus.Intended;
        payment.UpdatedAt    = DateTime.UtcNow;

        payment.Events.Add(new PaymentEvent
        {
            CreatedAt  = DateTime.UtcNow,
            RawPayload = rawContent,
            Type       = PaymentEventType.PaymentIntentCreated
        });

        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Payment intent created successfully | PaymentId: {PaymentId} | IntentionId: {IntentionId} | Amount: {Amount} | Currency: {Currency} | AppointmentId: {AppointmentId}",
            payment.Id, payment.IntentionId, payment.Amount, payment.Currency, appointmentId);

        return payment.ClientSecret;
    }
}
