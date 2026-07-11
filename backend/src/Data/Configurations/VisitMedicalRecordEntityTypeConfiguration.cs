using Dento.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dento.Data.Configurations;

public class VisitMedicalRecordEntityTypeConfiguration : IEntityTypeConfiguration<VisitMedicalRecord>
{
    public void Configure(EntityTypeBuilder<VisitMedicalRecord> builder)
    {
        builder.HasOne(v => v.Appointment)
               .WithOne()
               .HasForeignKey<VisitMedicalRecord>(v => v.AppointmentId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(v => v.MedicalRecord)
               .WithMany(m => m.VisitRecords)
               .HasForeignKey(v => v.MedicalRecordId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
