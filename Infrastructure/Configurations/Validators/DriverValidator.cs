using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations.Validators
{
    public class DriverValidator : IEntityTypeConfiguration<Driver>
    {
        public void Configure(EntityTypeBuilder<Driver> builder)
        {
            // Configure CommissionRate property
            builder.Property(x => x.CommissionRate)
                   .IsRequired()
                   .HasColumnType("decimal(3, 2)")
                   .HasDefaultValue(0.70m);

            // Configure Shift property
            builder.Property(x => x.Shift)
                   .IsRequired()
                   .HasMaxLength(10);

            // Relationship configured on Order (Restrict delete)
            builder.HasIndex(x => x.PhoneNumber1);
        }
    }
}