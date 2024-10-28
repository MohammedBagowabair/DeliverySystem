using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations.Validators
{
    public class OrderValidator : IEntityTypeConfiguration<Order>
    {

        public void Configure(EntityTypeBuilder<Order> builder)
        {
            // Configure Customer property
            builder.HasOne(order => order.Customer)
                   .WithMany()
                   .IsRequired();  // Ensures that a Customer is always associated with an Order

            // Configure Driver property
            builder.HasOne(order => order.Driver)
                   .WithMany()
                   .IsRequired();  // Ensures that a Driver is always associated with an Order

            // Configure DeliveryTime property
            builder.Property(order => order.DeliveryTime)
                   .IsRequired();  // This ensures DeliveryTime is not nullable; future validation is handled by business logic

            // Configure PaymentMethod property
            builder.Property(order => order.PaymentMethod)
                   .IsRequired()
                   .HasMaxLength(50);  // Setting a reasonable max length for the payment method string

            // Configure DeliveryFee property
            builder.Property(order => order.DeliveryFee)
                   .HasColumnType("decimal(18,2)")  // Adjust precision and scale as needed
                   .IsRequired();  // Required based on the FluentValidation rule

            // Configure CouponDiscount property
            builder.Property(order => order.CouponDiscount)
                   .HasColumnType("decimal(18,2)")  // Adjust precision and scale as needed
                   .IsRequired();  // Required based on the FluentValidation rule

            // Configure FinalPrice property
            builder.Property(order => order.FinalPrice)
                   .HasColumnType("decimal(18,2)")  // Adjust precision and scale as needed
                   .IsRequired();  // Ensures FinalPrice is not nullable

            // Configure OrderStatus property
            builder.Property(order => order.OrderStatus)
                   .IsRequired()
                   .HasMaxLength(50);  // Assuming a max length for OrderStatus string

            // Configure Notice property
            builder.Property(order => order.Notice)
                   .HasMaxLength(500);  // Limits the length of the Notice to 500 characters

            builder.HasOne<Driver>(x => x.Driver).WithMany(x => x.Orders).HasForeignKey(x => x.DriverId).OnDelete(DeleteBehavior.Cascade);

            builder.HasOne<Customer>(x => x.Customer).WithMany(x => x.Orders).HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Cascade);
        }
    }

}
