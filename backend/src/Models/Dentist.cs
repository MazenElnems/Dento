namespace Dento.Models;

public class Dentist : ApplicationUser
{
    public string Specialty { get; set; } = default!;
    public int YearsOfExperience { get; set; }
    public DentistAvailability DentistAvailability { get; set; } = default!;
    public decimal? ConsultationFee { get; set; }
    public List<Appointment> Appointments { get; set; } = [];

    public void BuildDefaultSchedule()
    {
        DentistAvailability = new DentistAvailability
        {
            CreatedAt = DateTime.UtcNow,
            SAT = false,
            SUN = false,
            MON = false,
            TUE = false,
            WED = false,
            THU = false,
            FRI = false,
            IsActive = false,
            FromHour = new TimeOnly(0),
            ToHour = new TimeOnly(0),
            SlotLengthInMinutes = 60,
            DentistId = Id
        };
    }
}
