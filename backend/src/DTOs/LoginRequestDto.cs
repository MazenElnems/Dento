using System.ComponentModel.DataAnnotations;

namespace Dento.DTOs;

public class LoginRequestDto
{
    [Required, EmailAddress, MaxLength(255)]
    public required string Email { get; init; }
    [Required, MaxLength(255), MinLength(6)]
    public required string Password { get; init; }
}
