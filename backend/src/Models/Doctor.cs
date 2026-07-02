using Dento.Data.Entities;

namespace Dento.Models
{
    public class Doctor
    {
            public string ApplicationUserId { get; set; } = default!;

            public ApplicationUser User { get; set; } = default!;

            public string Specialty { get; set; } = default!;
    }
}
