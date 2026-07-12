using Dento.Enums;

namespace Dento.DTOs;

public class MedicalHistoryDto
{
    public string Id { get; set; } = string.Empty;
    public string MedicalRecordId { get; set; } = string.Empty;

    public List<string> MedicalConditions { get; set; } = [];
    public List<string> Allergies { get; set; } = [];
    
    public PregnancyStatus PregnancyStatus { get; set; }
    public SmokingStatus SmokingStatus { get; set; }
    
    public bool BleedingDisorders { get; set; }
    public bool HeartConditions { get; set; }
    public bool Diabetes { get; set; }
    public bool HighBloodPressure { get; set; }
    
    public string? MedicalNotes { get; set; }
}
