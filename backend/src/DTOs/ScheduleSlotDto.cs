namespace Dento.DTOs;

public class ScheduleSlotDto
{
    public string Id { get; set; } = default!;
    public TimeOnly From { get; set; }
    public TimeOnly To { get; set; }    
}
