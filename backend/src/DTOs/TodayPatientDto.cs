using Dento.Enums;

namespace Dento.DTOs;

public class TodayPatientDto
{
    public string AppointmentId { get; set; } = default!;
    public string PatientId { get; set; } = default!;
    public string FullName { get; set; } = default!;
    public string? PhoneNumber { get; set; }
    public TimeOnly AppointmentTime { get; set; }
    public AppointmentType AppointmentType { get; set; }
    public AppointmentStatus Status { get; set; }
}