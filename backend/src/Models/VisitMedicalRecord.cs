using System.ComponentModel.DataAnnotations.Schema;

namespace Dento.Models;

public class VisitMedicalRecord
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [ForeignKey(nameof(MedicalRecord))]
    public string MedicalRecordId { get; set; } = default!;
    public MedicalRecord MedicalRecord { get; set; } = default!;

    [ForeignKey(nameof(Appointment))]
    public string AppointmentId { get; set; } = default!;
    public Appointment Appointment { get; set; } = default!;

    public string? Diagnosis { get; set; }

    public ICollection<Prescription> Prescriptions { get; set; } = [];
    public ICollection<Procedure> Procedures { get; set; } = [];

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
