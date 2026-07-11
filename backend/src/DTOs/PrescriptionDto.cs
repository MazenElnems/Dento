namespace Dento.DTOs;

public class PrescriptionDto
{
    public string Id { get; set; } = string.Empty;
    public string MedicationName { get; set; } = string.Empty;
    public string Dosage { get; set; } = string.Empty;
    public string? Notes { get; set; }
}
