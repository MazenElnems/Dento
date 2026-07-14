using System.ComponentModel.DataAnnotations;

namespace Dento.DTOs;

public class ChangeNameDto
{
    [Required]
    [MaxLength(50)]
    public string FirstName { get; set; } = null!;

    [Required]
    [MaxLength(50)]
    public string MiddleName { get; set; } = null!;

    [Required]
    [MaxLength(50)]
    public string LastName { get; set; } = null!;
}
