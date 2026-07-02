using Dento.Constants;
using Dento.Data.Entities;
using Dento.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace Dento.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }



    public DbSet<Doctor> Doctors { get; set; }



    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);


        builder.Entity<Doctor>()
        .HasKey(d => d.ApplicationUserId);

        builder.Entity<Doctor>()
            .HasOne(d => d.User)
            .WithOne(u => u.Doctor)
            .HasForeignKey<Doctor>(d => d.ApplicationUserId);


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


