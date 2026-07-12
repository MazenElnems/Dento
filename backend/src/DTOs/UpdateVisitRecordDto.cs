namespace Dento.DTOs;

public class UpdateVisitRecordDto
{
    public string? Diagnosis { get; set; }
    public List<CreatePrescriptionDto> Prescriptions { get; set; } = [];
    public List<CreateProcedureDto> Procedures { get; set; } = [];
}
