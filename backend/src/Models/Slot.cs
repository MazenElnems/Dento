using Dento.Enums;

namespace Dento.Models;

public class Slot
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public DentistAvailability DentistAvailability { get; set; } = default!;
    public string DentistAvailabilityId { get; set; } = default!;

    public SlotStatus Status { get; set; } = SlotStatus.Available;

    public DateOnly Date { get; set; }
    public TimeOnly From { get; set; }
    public TimeOnly To { get; set; }
}
