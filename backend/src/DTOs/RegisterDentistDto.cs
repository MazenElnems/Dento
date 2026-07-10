using System.ComponentModel.DataAnnotations;

namespace Dento.DTOs;

public class RegisterDentistDto
{
    [Required]
    public required string FirstName { get; init; }
    [Required]
    public required string MiddleName { get; init; }
    [Required]
    public required string LastName  { get; init; }
    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public required string Email { get; init; }
    [Required]
    public required string Password { get; init; }
    [Required]
    public string Specialty { get; init; } = default!;
    public decimal? ConsultationFee { get; set; }
}