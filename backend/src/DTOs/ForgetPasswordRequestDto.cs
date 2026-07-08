using System.ComponentModel.DataAnnotations;

namespace Dento.DTOs;

public class ForgetPasswordRequestDto
{
    [Required , MaxLength(200), EmailAddress]
    public required string Email { get; init; } 
}
