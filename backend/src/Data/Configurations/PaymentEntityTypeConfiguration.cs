using Dento.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dento.Data.Configurations;

public class PaymentEntityTypeConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder
            .HasIndex(p => p.IdempotencyKey)
            .IsUnique();

        builder
            .Property(x => x.Status)
            .HasConversion<string>();
     
        builder
            .Property(x => x.PaymentMethod)
            .HasConversion<string>();
    }
}
