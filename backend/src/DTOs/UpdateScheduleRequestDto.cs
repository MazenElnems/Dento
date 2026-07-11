using System.ComponentModel.DataAnnotations;

namespace Dento.DTOs;

public class UpdateScheduleRequestDto
{
    [Required]
    public bool SAT { get; set; }
    [Required]
    public bool SUN { get; set; }
    [Required]
    public bool MON { get; set; }
    [Required]
    public bool TUE { get; set; }
    [Required]
    public bool WED { get; set; }
    [Required]
    public bool THU { get; set; }
    [Required]
    public bool FRI { get; set; }

    [Required]
    public TimeOnly FromHour { get; set; }
    [Required]
    public TimeOnly ToHour { get; set; }

    public TimeOnly? SecondFromHour { get; set; }
    public TimeOnly? SecondToHour { get; set; }

    [Required, Range(15, 120)]
    public int SlotLengthInMinutes { get; set; }
    public bool IsActive { get; set; }  
}
