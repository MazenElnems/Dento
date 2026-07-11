namespace Dento.DTOs;

public class DentistListItemDto
{
    public string Id { get; set; } = default!;
    public string FullName { get; set; } = default!;
    public string Specialty { get; set; } = default!;
    public decimal? ConsultationFee { get; set; }

    /// <summary>
    /// The ID of the dentist's availability/schedule — used to fetch the schedule in the next step.
    /// </summary>
    public string ScheduleId { get; set; } = default!;

    public string? ImageUrl { get; set; }
    public int YearsOfExperience { get; set; }
}
