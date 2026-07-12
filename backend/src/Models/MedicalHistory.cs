using Dento.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dento.Models;

public class MedicalHistory
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [ForeignKey(nameof(MedicalRecord))]
    public string MedicalRecordId { get; set; } = default!;
    public MedicalRecord MedicalRecord { get; set; } = default!;

    public List<string> MedicalConditions { get; set; } = [];
    public List<string> Allergies { get; set; } = [];
    
    public PregnancyStatus PregnancyStatus { get; set; } = PregnancyStatus.NotApplicable;
    public SmokingStatus SmokingStatus { get; set; } = SmokingStatus.Never;
    
    public bool BleedingDisorders { get; set; }
    public bool HeartConditions { get; set; }
    public bool Diabetes { get; set; }
    public bool HighBloodPressure { get; set; }
    
    public string? MedicalNotes { get; set; }
}
