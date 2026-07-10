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

    public async Task<string> CreatePaymentIntent(string appointmentId, string idempotencyKey)
    {
        var appointment = await _context.Appointments
            .Include(x => x.Dentist)
            .Include(x => x.Patient)
            .Include(x => x.Payment)
            .FirstOrDefaultAsync(x => x.Id == appointmentId);

        if (appointment == null)
            throw new ResourceNotFoundException(nameof(Appointment));

        // Already Payment Check
        if (appointment.Payment != null && appointment.Payment.Status == PaymentStatus.Pending)
            throw new AppointmentPaymentException("Payment already in-progress");

        if (appointment.Payment != null && appointment.Payment.Status == PaymentStatus.Paid)
            throw new AppointmentPaymentException("Payment already Completed");

        var payment = new Payment
        {
            IdempotencyKey = idempotencyKey,
            Amount = appointment.Dentist.ConsultationFee ?? 200,
            Currency = "EGP",
            PayerEmail = appointment.Patient.Email,
            PayerName = appointment.Patient.FullName,
            Status = PaymentStatus.Pending,
            PaymentMethod = PaymentMethod.Card,
            CreatedAt = DateTime.UtcNow,
        };

        appointment.PaymentId = payment.Id;
        _context.Payments.Add(payment);

        // first Insert Pending Payment Row To ensure Idempotency
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
            amount = appointment.Dentist.ConsultationFee, // or appointment.Dentist.ConsultationFee
            currency = "EGP",
            payment_methods = new[] { "card" },

            items = new[]
            {
                new
                {
                    name = "Dental Consultation",
                    amount = appointment.Dentist.ConsultationFee,
                    description = $"Consultation with Dr. {appointment.Dentist.FullName}",
                    quantity = 1
                }
            },

            billing_data = new
            {
                first_name = appointment.Patient.FirstName,
                last_name = appointment.Patient.LastName,
                email = appointment.Patient.Email,
                phone_number = appointment.Patient.PhoneNumber
            },

            special_reference = idempotencyKey,

            notification_url = $"{_appOptions.ApiBaseUrl}/{_paymob.WebhookEndpointUrl}",
            redirection_url = $"{_client.Host}/payment/result"
        };

        using var client = _factory.CreateClient();

        var url = $"{_paymob.BaseUrl}/{_paymob.CreatePaymentIntentPath}";

        HttpResponseMessage response;
        try
        {
            response = await client.PostAsJsonAsync(url, body);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or TimeoutException)
        {
            _logger.LogError(ex, "Network failure calling Paymob for payment {PaymentId}", payment.Id);

            payment.Status = PaymentStatus.Failed;
            payment.FauilerReason = "Network error contacting payment gateway";
            payment.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            throw new PaymentGatewayException(StatusCodes.Status502BadGateway, "Unable to reach payment gateway. Please try again.");
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Unknown Exception was thrown for payment {PaymentId}", payment.Id);

            payment.Status = PaymentStatus.Failed;
            payment.FauilerReason = "Network error contacting payment gateway";
            payment.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            throw;
        }

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError(
                "Failed to create payment intent. Status: {StatusCode} , Content: {@Content}",
                response.StatusCode,
                response.Content
            );

            payment.Status = PaymentStatus.Failed;
            payment.FauilerReason = "Failed to create payment intent";
            payment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            throw new PaymentGatewayException(StatusCodes.Status502BadGateway, "Unable to create payment. Please try again.");
        }

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        };

        var rawContent = await response.Content.ReadAsStringAsync();

        var paymentIntentResponse = new CreatePaymentIntentPaymobResponseDto();
        try
        {
            paymentIntentResponse = await response.Content
                .ReadFromJsonAsync<CreatePaymentIntentPaymobResponseDto>(options);
        }
        catch(JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse Paymob response for payment {PaymentId}. Raw: {Raw}", payment.Id, rawContent);
            
            paymentIntentResponse = null;
        }

        if (paymentIntentResponse is null)
        {
            payment.Status = PaymentStatus.Failed;
            payment.FauilerReason = "Unable to parse payment gateway response";
            payment.UpdatedAt = DateTime.UtcNow;

            payment.Events.Add(new PaymentEvent
            {
                CreatedAt = DateTime.UtcNow,
                RawPayload = rawContent,
                Type = PaymentEventType.PaymentIntentCreated
            });

            await _context.SaveChangesAsync();

            throw new PaymentGatewayException(StatusCodes.Status502BadGateway, "Unable to create payment. Please try again.");
        }

        payment.IntentionId = paymentIntentResponse.IntentionOrderId;
        payment.ClientSecret = paymentIntentResponse.ClientSecret;
        payment.Status = PaymentStatus.Intended;
        payment.UpdatedAt = DateTime.UtcNow;

        payment.Events.Add(new PaymentEvent
        {
            CreatedAt = DateTime.UtcNow,
            RawPayload = rawContent,
            Type = PaymentEventType.PaymentIntentCreated
        });

        await _context.SaveChangesAsync();

        return payment.ClientSecret;
    }
}
