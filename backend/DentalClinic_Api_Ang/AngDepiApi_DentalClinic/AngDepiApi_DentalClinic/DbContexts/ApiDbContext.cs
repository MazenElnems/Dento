using AngDepiApi_DentalClinic.Consts;
using AngDepiApi_DentalClinic.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AngDepiApi_DentalClinic.DbContexts
{


    public class ApiDbContext : IdentityDbContext<AppUser>
    {
        public ApiDbContext(DbContextOptions<ApiDbContext> options) : base(options)
        {
        }



        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<IdentityRole>().HasData(
            new IdentityRole
            {
                Id = "1",
                Name = RolesNames.Admin,
                NormalizedName = RolesNames.Admin.ToUpper()
            },
            new IdentityRole
            {
                Id = "2",
                Name = RolesNames.Doctor,
                NormalizedName = RolesNames.Doctor.ToUpper()
            },
            new IdentityRole
            {
                Id = "3",
                Name = RolesNames.Patient,
                NormalizedName = RolesNames.Patient.ToUpper()
            },
            new IdentityRole
            {
                Id = "4",
                Name = RolesNames.Receptionist,
                NormalizedName = RolesNames.Receptionist.ToUpper()
            }
        );
        }

    }

    
}
