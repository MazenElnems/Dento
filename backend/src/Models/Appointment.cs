using Dento.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dento.Models;

public class Appointment
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [ForeignKey(nameof(Slot))]
    public string SlotId { get; set; } = default!;
    public Slot Slot { get; set; } = default!;

    public AppointmentStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public DateTime? CanceledAt { get; set; }
    public string? CancelationReason { get; set; }

    [ForeignKey(nameof(Patient))]
    public string PatientId { get; set; } = default!;
    public Patient Patient { get; set; } = default!;
}
