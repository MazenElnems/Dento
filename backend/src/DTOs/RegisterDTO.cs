using System.ComponentModel.DataAnnotations;

namespace Dento.DTOs;

public class RegisterDTO
{

    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string? Email { get; set; }
    [Required]
    public string? Password { get; set; }

}
