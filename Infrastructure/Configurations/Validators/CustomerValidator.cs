using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations.Validators
{
    public class CustomerValidator : IEntityTypeConfiguration<Customer>
    {
        public void Configure(EntityTypeBuilder<Customer> builder)
        {
            builder.HasMany(customer => customer.Orders)
                          .WithOne(order => order.Customer)
                          .IsRequired();  // Ensures each Order has an associated Customer


            builder.HasMany<Order>(x => x.Orders).WithOne(x => x.Customer).HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Cascade);

        }
    }
}