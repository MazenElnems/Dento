using Dento.Constants;
using Dento.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Dento.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }



    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<IdentityRole>().HasData(
        new IdentityRole
        {
            Id = "1",
            Name = RoleNames.Admin,
            NormalizedName = RoleNames.Admin.ToUpper()
        },
        new IdentityRole
        {
            Id = "2",
            Name = RoleNames.Doctor,
            NormalizedName = RoleNames.Doctor.ToUpper()
        },
        new IdentityRole
        {
            Id = "3",
            Name = RoleNames.Patient,
            NormalizedName = RoleNames.Patient.ToUpper()
        },
        new IdentityRole
        {
            Id = "4",
            Name = RoleNames.Receptionist,
            NormalizedName = RoleNames.Receptionist.ToUpper()
        }
    );
    }

}


