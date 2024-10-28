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
                   .IsRequired()  // Ensure it's not nullable
                   .HasColumnType("decimal(3, 2)")  // Precision for values between 0.00 and 1.00
                   .HasDefaultValue(0);  // Set a default if needed

            // Configure Shift property
            builder.Property(x => x.Shift)
                   .IsRequired()  // Ensure it's not nullable
                   .HasMaxLength(10);  // Set max length to prevent long strings

            builder.HasMany<Order>(x => x.Orders).WithOne(x => x.Driver).HasForeignKey(x => x.DriverId).OnDelete(DeleteBehavior.Cascade);

        }
    }
}