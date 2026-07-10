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

    public PaymentsController(IPaymentService paymentService, IOptions<PaymobSettings> paymob)
    {
        _paymentService = paymentService;
        _paymob = paymob.Value;
    }

    [HttpPost("create-payment-intent")]
    [Authorize(Roles = RoleNames.Patient)] 
    public async Task<ActionResult<ApiResponse>> CreatePaymentIntent(CreatePaymentIntentRequestDto request)
    {
        var clientSecret = _paymentService.CreatePaymentIntent(request.AppointmentId, request.IdempotencyKey);

        return ApiResponse.SuccessResponse(new
        {
            request.AppointmentId,
            ClientSecret = clientSecret,
            _paymob.PublicKey
        }, "Payment intent created successfully");
    }
}
