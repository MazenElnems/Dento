namespace Dento.Models;

public class Dentist : ApplicationUser
{
    public string Specialty { get; set; } = default!;
    public int YearsOfExperience { get; set; }  
}
