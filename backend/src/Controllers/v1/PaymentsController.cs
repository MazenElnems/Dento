using Asp.Versioning;
using Dento.Constants;
using Dento.Controllers.Common;
using Dento.DTOs;
using Dento.Enums;
using Dento.Options;
using Dento.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;

namespace Dento.Controllers.v1;

[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
[ApiVersion("1.0")]
public class PaymentsController : BaseApiController
{
    private readonly IPaymentService _paymentService;
    private readonly PaymobSettings _paymob;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(IPaymentService paymentService, IOptions<PaymobSettings> paymob, ILogger<PaymentsController> logger)
    {
        _paymentService = paymentService;
        _paymob = paymob.Value;
        _logger = logger;
    }

    /// <summary>
    /// Creates a payment for an appointment (online or cash).
    /// </summary>
    /// <remarks>
    /// For online payments, returns a client secret and public key for Paymob checkout.
    /// For cash payments, creates a pending payment and confirms the appointment immediately.
    /// </remarks>
    [HttpPost("create-payment")]
    [Authorize(Roles = RoleNames.Patient)]
    [SwaggerOperation(Summary = "Create payment", Description = "Creates a payment using the specified payment type (Online or Cash).")]
    public async Task<ActionResult<ApiResponse>> CreatePayment(CreatePaymentIntentRequestDto request)
    {
        _logger.LogInformation("Payment creation requested | AppointmentId: {AppointmentId} | PaymentType: {PaymentType} | UserId: {UserId}",
            request.AppointmentId, request.PaymentType, CurrentUser.Id);

        var result = await _paymentService.CreatePaymentAsync(
            request.AppointmentId, request.IdempotencyKey, CurrentUser.Id, request.PaymentType);

        if (result.PaymentMethod == PaymentMethod.Cash)
        {
            return ApiResponse.SuccessResponse(new
            {
                result.PaymentId,
                result.Status,
                result.PaymentMethod
            }, "Cash payment created. Appointment confirmed. Pay at the clinic.");
        }

        // Online payment — include checkout info
        Response.Headers.Location = $"{_paymob.BaseUrl}/{_paymob.CheckoutPageUrl}?publicKey={result.PublicKey}&clientSecret={result.ClientSecret}";

        return ApiResponse.SuccessResponse(new
        {
            request.AppointmentId,
            result.ClientSecret,
            result.PublicKey,
            result.PaymentMethod
        }, "Payment intent created successfully.");
    }

    /// <summary>
    /// Legacy endpoint — creates an online payment intent.
    /// </summary>
    [HttpPost("create-payment-intent")]
    [Authorize(Roles = RoleNames.Patient)]
    [SwaggerOperation(Summary = "Create online payment intent (legacy)", Description = "Creates an online payment intent via Paymob. Use 'create-payment' for new integrations.")]
    public async Task<ActionResult<ApiResponse>> CreatePaymentIntent(CreatePaymentIntentRequestDto request)
    {
        _logger.LogInformation("Legacy payment intent creation requested | AppointmentId: {AppointmentId} | UserId: {UserId}",
            request.AppointmentId, CurrentUser.Id);

        var result = await _paymentService.CreatePaymentAsync(
            request.AppointmentId, request.IdempotencyKey, CurrentUser.Id, PaymentType.Online);

        Response.Headers.Location = $"{_paymob.BaseUrl}/{_paymob.CheckoutPageUrl}?publicKey={result.PublicKey}&clientSecret={result.ClientSecret}";

        return ApiResponse.SuccessResponse(new
        {
            request.AppointmentId,
            ClientSecret = result.ClientSecret,
            PublicKey = result.PublicKey
        }, "Payment intent created successfully");
    }

    /// <summary>
    /// Confirms a cash payment after the patient has paid at the clinic.
    /// </summary>
    /// <remarks>
    /// Only Receptionists can confirm cash payments.
    /// The payment must have PaymentMethod = Cash and Status = Pending.
    /// </remarks>
    /// <param name="paymentId">The ID of the cash payment to confirm.</param>
    [HttpPost("{paymentId}/confirm-cash")]
    [Authorize(Roles = RoleNames.Receptionist)]
    [SwaggerOperation(Summary = "Confirm cash payment", Description = "Marks a pending cash payment as paid. Receptionist only.")]
    public async Task<ActionResult<ApiResponse>> ConfirmCashPayment(string paymentId)
    {
        _logger.LogInformation("Cash payment confirmation requested | PaymentId: {PaymentId} | ReceptionistId: {ReceptionistId}",
            paymentId, CurrentUser.Id);

        var result = await _paymentService.ConfirmCashPaymentAsync(paymentId, CurrentUser.Id);

        return ApiResponse.SuccessResponse(result, "Cash payment confirmed successfully.");
    }
}
