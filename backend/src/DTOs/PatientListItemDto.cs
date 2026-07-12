using Dento.Enums;

namespace Dento.DTOs;

public class PatientListItemDto
{
    public string Id { get; set; } = default!;
    public string FullName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string? PhoneNumber { get; set; }
    public Gender Gender { get; set; }
    public DateOnly DateOfBirth { get; set; }
    public int Age { get; set; }
    public string? MedicalRecordId { get; set; }
}
