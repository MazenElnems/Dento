using System.ComponentModel.DataAnnotations;

namespace Dento.DTOs;

public class LoginDTO
{
    [Required]
    [MaxLength(255)]
    [EmailAddress]
    public string? Email { get; set; }
    [Required]
    public string? Password { get; set; }
}
