using Dento.Enums;
using System.ComponentModel.DataAnnotations;

namespace Dento.Models;

public class Slot
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public DentistAvailability? DentistAvailability { get; set; }
    public string? DentistAvailabilityId { get; set; }

    public SlotStatus Status { get; set; } = SlotStatus.Available;
    public DateTime? LockedUntil { get; set; }  

    public DateOnly Date { get; set; }
    public TimeOnly From { get; set; }
    public TimeOnly To { get; set; }

    [Timestamp]
    public byte[] RowVersion { get; set; } = default!;
}
