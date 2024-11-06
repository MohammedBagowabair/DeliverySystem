using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations.Validators
{

    public class UserValidator : IEntityTypeConfiguration<User>
    {


        public void Configure(EntityTypeBuilder<User> builder)
        {
            // Configure Email property
            builder.Property(user => user.Email)
                   .IsRequired()  // Ensures Email is not nullable
                   .HasMaxLength(100);  // Sets max length to 100 characters

            builder.HasIndex(user => user.Email).IsUnique();

            // Configure PasswordHash property
            builder.Property(user => user.PasswordHash)
                   .IsRequired()  // Ensures PasswordHash is not nullable
                   .HasMaxLength(256);  // Sets a max length, assuming a typical hash length (adjust as needed)

            // Configure Role property
            builder.Property(user => user.Role)
                   .IsRequired()  // Ensures Role is not nullable
                   .HasMaxLength(20);  // Set a max length suitable for roles (e.g., 20 characters)
        }
    }

}
