using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations.Validators
{
    public class OrderValidator : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.Property(order => order.DeliveryTime)
                   .IsRequired();

            builder.Property(order => order.PaymentMethod)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.Property(order => order.DeliveryFee)
                   .HasColumnType("decimal(18,2)")
                   .IsRequired();

            builder.Property(order => order.CouponDiscount)
                   .HasColumnType("decimal(18,2)")
                   .IsRequired();

            builder.Property(order => order.FinalPrice)
                   .HasColumnType("decimal(18,2)")
                   .IsRequired();

            builder.Property(order => order.orderStatus)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.Property(order => order.Notice)
                   .HasMaxLength(500);

            // Restrict: deleting a driver/customer must not wipe historical orders
            builder.HasOne(x => x.Driver)
                   .WithMany(x => x.Orders)
                   .HasForeignKey(x => x.DriverId)
                   .OnDelete(DeleteBehavior.Restrict)
                   .IsRequired();

            builder.HasOne(x => x.Customer)
                   .WithMany(x => x.Orders)
                   .HasForeignKey(x => x.CustomerId)
                   .OnDelete(DeleteBehavior.Restrict)
                   .IsRequired();

            builder.HasIndex(x => x.CustomerId);
            builder.HasIndex(x => x.DriverId);
        }
    }
}
