using System.ComponentModel.DataAnnotations;

namespace AngDepiApi_DentalClinic.DTOs
{
    public class RegisterDTO
    {

        [Required]
        [EmailAddress]
        [MaxLength(255)]
        public string? Email { get; set; }
        [Required]
        public string? Password { get; set; }

    }
}
