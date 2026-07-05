using System.ComponentModel.DataAnnotations;

namespace Dento.DTOs;

public class ResetPasswordRequestDto
{
    [Required]
    public required string UserId { get; init; }
    [Required]
    public required string Token { get; init; }
    [Required, MinLength(6)]
    public required string NewPassword { get; init; }
}
