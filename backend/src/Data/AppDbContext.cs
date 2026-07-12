using Dento.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Dento.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public DbSet<Dentist> Dentists { get; set; }
    public DbSet<Patient> Patients { get; set; }
    public DbSet<Admin> Admins { get; set; }
    public DbSet<EmailVerificationCode> EmailVerificationCodes { get; set; }
    public DbSet<Receptionist> Receptionists { get; set; }
    public DbSet<DentistAvailability> DentistAvailability { get; set; }
    public DbSet<Appointment> Appointments { get; set; }
    public DbSet<Slot> Slots { get; set; }
    public DbSet<Payment> Payments { get; set; }    
    public DbSet<MedicalRecord> MedicalRecords { get; set; }
    public DbSet<MedicalHistory> MedicalHistories { get; set; }
    public DbSet<VisitMedicalRecord> VisitMedicalRecords { get; set; }
    public DbSet<Prescription> Prescriptions { get; set; }
    public DbSet<Procedure> Procedures { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
