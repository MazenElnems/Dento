using Dento.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dento.Data.Configurations;

public class DentistEntityTypeConfigurations : IEntityTypeConfiguration<Dentist>
{
    public void Configure(EntityTypeBuilder<Dentist> builder)
    {
        builder.ToTable("Dentists");
    }
}
