using Dento.Enums;
using System.ComponentModel.DataAnnotations;

namespace Dento.DTOs;

public class RegisterPatientRequestDto
{
    [Required, MaxLength(200)]
    public required string FirstName { get; init; }

    [Required, MaxLength(200)]
    public required string MiddleName { get; init; }

    [Required, MaxLength(200)]
    public required string LastName { get; init; }

    [Required, MaxLength(11), Phone]
    public required string Phone { get; init; }

    [Required]
    public Gender Gender { get; set; }

    [Required]
    public DateOnly BirthDate { get; init; }

    [Required, EmailAddress, MaxLength(255)]
    public required string Email { get; init; }

    [Required , MinLength(6)]
    public required string Password { get; init; }
}
