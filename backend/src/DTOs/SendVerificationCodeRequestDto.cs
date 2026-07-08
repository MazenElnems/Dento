using System.ComponentModel.DataAnnotations;

namespace Dento.DTOs;

public class SendVerificationCodeRequestDto
{
    [Required]
    public required string Email { get; init; }
}
