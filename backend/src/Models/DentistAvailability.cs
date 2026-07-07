using System.ComponentModel.DataAnnotations.Schema;

namespace Dento.Models;

/// <summary>
/// Represents the availability of a dentist for appointments.
/// </summary>
public class DentistAvailability 
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public Dentist Dentist { get; set; } = default!;
    public string DentistId { get; set; } = default!;

    public bool SAT { get; set; }
    public bool SUN { get; set; }
    public bool MON { get; set; }
    public bool TUE { get; set; }
    public bool WED { get; set; }
    public bool THU { get; set; }
    public bool FRI { get; set; }

    public TimeOnly FromHour { get; set; }
    public TimeOnly ToHour { get; set; }

    public bool HasTwoShifts => SecondToHour.HasValue && SecondToHour.HasValue;

    public TimeOnly? SecondFromHour { get; set; }
    public TimeOnly? SecondToHour { get; set; }

    public int SlotLengthInMinutes { get; set; }
    
    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public List<Slot> Slots { get; set; } = [];
}
