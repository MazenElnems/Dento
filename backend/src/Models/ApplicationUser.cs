using Dento.Models;
using Microsoft.AspNetCore.Identity;

namespace Dento.Data.Entities;

public class ApplicationUser : IdentityUser
{
    public Doctor? Doctor { get; set; }
}
