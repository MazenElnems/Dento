using System.ComponentModel.DataAnnotations.Schema;

namespace Dento.Models;

public class Prescription
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [ForeignKey(nameof(VisitMedicalRecord))]
    public string VisitMedicalRecordId { get; set; } = default!;
    public VisitMedicalRecord VisitMedicalRecord { get; set; } = default!;

    public string MedicationName { get; set; } = string.Empty;
    public string Dosage { get; set; } = string.Empty;
    public string? Notes { get; set; }
}
