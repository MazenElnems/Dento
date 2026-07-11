namespace Dento.DTOs;

public class VisitMedicalRecordDto
{
    public string Id { get; set; } = string.Empty;
    public string AppointmentId { get; set; } = string.Empty;
    public string? Diagnosis { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<PrescriptionDto> Prescriptions { get; set; } = [];
    public List<ProcedureDto> Procedures { get; set; } = [];
}
