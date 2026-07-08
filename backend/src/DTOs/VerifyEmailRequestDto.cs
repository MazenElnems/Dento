using System.ComponentModel.DataAnnotations;

namespace Dento.DTOs;

public class VerifyEmailRequestDto
{
    [Required]
    public required string Code { get; init; } 
    [Required]
    public required string UserId { get; init; }
}
