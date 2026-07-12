using System.ComponentModel.DataAnnotations.Schema;

namespace Dento.Models;

public class MedicalRecord
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [ForeignKey(nameof(Patient))]
    public string PatientId { get; set; } = default!;
    public Patient Patient { get; set; } = default!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<VisitMedicalRecord> VisitRecords { get; set; } = [];
    public MedicalHistory MedicalHistory { get; set; } = default!;
}
