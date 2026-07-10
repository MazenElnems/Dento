using Asp.Versioning;
using Dento.Constants;
using Dento.Controllers.Common;
using Dento.DTOs;
using Dento.Options;
using Dento.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

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

    [HttpPost("create-payment-intent")]
    [Authorize(Roles = RoleNames.Patient)] 
    public async Task<ActionResult<ApiResponse>> CreatePaymentIntent(CreatePaymentIntentRequestDto request)
    {
        _logger.LogInformation("Payment intent creation requested | AppointmentId: {AppointmentId} | UserId: {UserId} | IdempotencyKey: {IdempotencyKey}",
            request.AppointmentId, CurrentUser.Id, request.IdempotencyKey);

        var clientSecret = _paymentService.CreatePaymentIntent(request.AppointmentId, request.IdempotencyKey);

        _logger.LogInformation("Payment intent created — client secret returned | AppointmentId: {AppointmentId} | UserId: {UserId}",
            request.AppointmentId, CurrentUser.Id);

        return ApiResponse.SuccessResponse(new
        {
            request.AppointmentId,
            ClientSecret = clientSecret,
            _paymob.PublicKey
        }, "Payment intent created successfully");
    }
}
