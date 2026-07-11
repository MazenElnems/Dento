using Dento.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dento.Data.Configurations;

public class MedicalRecordEntityTypeConfiguration : IEntityTypeConfiguration<MedicalRecord>
{
    public void Configure(EntityTypeBuilder<MedicalRecord> builder)
    {
        builder.HasOne(m => m.Patient)
               .WithOne(p => p.MedicalRecord)
               .HasForeignKey<MedicalRecord>(m => m.PatientId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
