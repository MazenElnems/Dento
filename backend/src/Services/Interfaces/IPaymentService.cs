using Dento.DTOs;
using Dento.Enums;
using Dento.Models;

namespace Dento.Services.Interfaces;

public interface IPaymentService
{
    Task<string> CreatePaymentIntent(string appointmentId, string idempotencyKey, string patientId);
}
