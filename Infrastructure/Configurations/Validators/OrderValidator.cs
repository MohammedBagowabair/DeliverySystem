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

            builder.Property(order => order.DiscountType)
                   .IsRequired()
                   .HasDefaultValue(Domain.Enums.DiscountType.None);

            builder.Property(order => order.DiscountValue)
                   .HasColumnType("decimal(18,2)")
                   .IsRequired()
                   .HasDefaultValue(0m);

            builder.Property(order => order.DiscountAmount)
                   .HasColumnType("decimal(18,2)")
                   .IsRequired()
                   .HasDefaultValue(0m);

            builder.Property(order => order.DriverCommissionRate)
                   .HasColumnType("decimal(5,4)")
                   .IsRequired()
                   .HasDefaultValue(0m);

            builder.Property(order => order.DriverAmount)
                   .HasColumnType("decimal(18,2)")
                   .IsRequired()
                   .HasDefaultValue(0m);

            builder.Property(order => order.CompanyGrossAmount)
                   .HasColumnType("decimal(18,2)")
                   .IsRequired()
                   .HasDefaultValue(0m);

            builder.Property(order => order.CompanyNetAmount)
                   .HasColumnType("decimal(18,2)")
                   .IsRequired()
                   .HasDefaultValue(0m);

            builder.Property(order => order.FinalPrice)
                   .HasColumnType("decimal(18,2)")
                   .IsRequired();

            builder.Property(order => order.orderStatus)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.Property(order => order.Notice)
                   .HasMaxLength(500);

            builder.Property(order => order.DriverName)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(order => order.CustomerName)
                   .IsRequired()
                   .HasMaxLength(200);

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

            builder.HasOne(x => x.CreatedByUser)
                   .WithMany()
                   .HasForeignKey(x => x.CreatedByUserId)
                   .OnDelete(DeleteBehavior.SetNull)
                   .IsRequired(false);

            builder.HasIndex(x => x.CustomerId);
            builder.HasIndex(x => x.DriverId);
            builder.HasIndex(x => x.CreatedByUserId);
            builder.HasIndex(x => x.DeliveryTime);
            builder.HasIndex(x => x.orderStatus);
            builder.HasIndex(x => new { x.DeliveryTime, x.orderStatus });
        }
    }
}
