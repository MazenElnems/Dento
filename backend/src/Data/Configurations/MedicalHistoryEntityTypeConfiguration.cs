using Dento.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dento.Data.Configurations;

public class MedicalHistoryEntityTypeConfiguration : IEntityTypeConfiguration<MedicalHistory>
{
    public void Configure(EntityTypeBuilder<MedicalHistory> builder)
    {
        builder.HasOne(mh => mh.MedicalRecord)
               .WithOne(mr => mr.MedicalHistory)
               .HasForeignKey<MedicalHistory>(mh => mh.MedicalRecordId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
