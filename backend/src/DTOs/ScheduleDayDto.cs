namespace Dento.DTOs;

public class ScheduleDayDto
{
    public DateOnly Date { get; set; }
    public List<ScheduleSlotDto> Slots { get; set; } = [];
}
