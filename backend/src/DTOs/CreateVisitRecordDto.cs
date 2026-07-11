using System.ComponentModel.DataAnnotations;

namespace Dento.DTOs;

public class CreateVisitRecordDto
{
    [Required]
    public string AppointmentId { get; set; } = string.Empty;
    public string? Diagnosis { get; set; }
    
    public List<CreatePrescriptionDto> Prescriptions { get; set; } = [];
    public List<CreateProcedureDto> Procedures { get; set; } = [];
}

public class CreatePrescriptionDto
{
    [Required]
    public string MedicationName { get; set; } = string.Empty;
    [Required]
    public string Dosage { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

public class CreateProcedureDto
{
    [Required]
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
