using System.ComponentModel.DataAnnotations;

namespace Dento.DTOs;

public class UpdateDentistProfileDto
{
    [Required]
    [MaxLength(100)]
    public string Specialty { get; set; } = null!;

    [Range(0, double.MaxValue, ErrorMessage = "Consultation fee must be a positive value.")]
    public decimal? ConsultationFee { get; set; }
}
