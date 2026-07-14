using System.ComponentModel.DataAnnotations;

namespace Dento.DTOs;

public class ChangePasswordDto
{
    [Required]
    public string CurrentPassword { get; set; } = null!;

    [Required]
    [MinLength(6, ErrorMessage = "The password must be at least 6 characters long.")]
    public string NewPassword { get; set; } = null!;
}
