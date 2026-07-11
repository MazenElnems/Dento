namespace Dento.DTOs;

public class MedicalRecordDto
{
    public string Id { get; set; } = string.Empty;
    public string PatientId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<VisitMedicalRecordDto> VisitRecords { get; set; } = [];
}
