using Dento.Enums;
using System.ComponentModel.DataAnnotations;

namespace Dento.DTOs;

public class CreatePaymentIntentRequestDto
{
    [Required]
    public required string AppointmentId { get; init; }
    [Required]
    public required string IdempotencyKey { get; set; }
}
