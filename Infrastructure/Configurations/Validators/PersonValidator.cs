using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations.Validators
{
    public class PersonValidator : IEntityTypeConfiguration<Person>
    {
        public void Configure(EntityTypeBuilder<Person> builder)
        {
            // Configure FullName property
            builder.Property(person => person.FullName)
                   .IsRequired()  // Ensures it's not nullable
                   .HasMaxLength(50);  // Sets max length to 50 characters

            // Configure PhoneNumber1 property
            builder.Property(person => person.PhoneNumber1)
                   .IsRequired()  // Ensures it's not nullable
                   .HasMaxLength(15);  // Sets max length to 15 characters

            // Configure PhoneNumber2 property
            builder.Property(person => person.PhoneNumber2)
                   .HasMaxLength(15);  // Sets max length to 15 characters but nullable by default

            // Configure Address property
            builder.Property(person => person.Address)
                   .IsRequired();  // Ensures it's not nullable

            builder.Property(person => person.IsActive)
                .IsRequired()  // Ensures it's not nullable
                .HasDefaultValue(true);  // Sets default value to true
        }
    }
}
