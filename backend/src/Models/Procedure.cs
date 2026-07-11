using System.ComponentModel.DataAnnotations.Schema;

namespace Dento.Models;

public class Procedure
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [ForeignKey(nameof(VisitMedicalRecord))]
    public string VisitMedicalRecordId { get; set; } = default!;
    public VisitMedicalRecord VisitMedicalRecord { get; set; } = default!;

    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
