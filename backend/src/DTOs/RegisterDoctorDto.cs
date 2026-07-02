using System.ComponentModel.DataAnnotations;

namespace Dento.DTOs;

public class RegisterDoctorDto
{
    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string? Email { get; set; }
    [Required]
    public string? Password { get; set; }
    [Required]
    public string Specialty { get; set; } = default!;
}