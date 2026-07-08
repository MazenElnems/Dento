namespace Dento.DTOs;

public class DentistScheduleResponseDto
{
    public TimeZoneInfo TimeZone { get; set; } = default!;
    public List<ScheduleDayDto> Schedule { get; set; } = default!;
}
