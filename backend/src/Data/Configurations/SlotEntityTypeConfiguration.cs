using Dento.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dento.Data.Configurations;

public class SlotEntityTypeConfiguration : IEntityTypeConfiguration<Slot>
{
    public void Configure(EntityTypeBuilder<Slot> builder)
    {
        builder
            .Property(x => x.Status)
            .HasConversion<string>();

        builder
            .HasOne(s => s.DentistAvailability)
            .WithMany(a => a.Slots)
            .HasForeignKey(x => x.DentistAvailabilityId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
