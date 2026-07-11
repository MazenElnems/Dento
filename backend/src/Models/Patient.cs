using Dento.Enums;

namespace Dento.Models;

public class Patient : ApplicationUser
{
    public DateOnly DateOfBirth { get; set; }
    public int Age => DateTime.UtcNow.Year - DateOfBirth.Year;
    public Gender Gender { get; set; }
    public List<EmailVerificationCode> EmailVerificationCodes { get; set; } = [];
    public List<Appointment> Appointments { get; set; } = [];
    public MedicalRecord? MedicalRecord { get; set; }
}
