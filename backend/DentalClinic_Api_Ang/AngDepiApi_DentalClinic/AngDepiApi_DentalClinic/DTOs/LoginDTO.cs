using System.ComponentModel.DataAnnotations;

namespace AngDepiApi_DentalClinic.DTOs
{
    public class LoginDTO
    {
        [Required]
        [MaxLength(255)]
        [EmailAddress]
        public string? Email { get; set; }
        [Required]
        public string? Password { get; set; }
    }
}
