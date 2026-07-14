namespace Dento.DTOs;

public class UserProfileDto
{
    public string Id { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string MiddleName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string? PhoneNumber { get; set; }
    public string Role { get; set; } = null!;

    // Dentist-specific fields (populated only when role is Dentist)
    public string? Specialty { get; set; }
    public decimal? ConsultationFee { get; set; }
}

