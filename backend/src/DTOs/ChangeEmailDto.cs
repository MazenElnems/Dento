using System.ComponentModel.DataAnnotations;

namespace Dento.DTOs;

public class ChangeEmailDto
{
    [Required]
    [EmailAddress]
    public string NewEmail { get; set; } = null!;
}
