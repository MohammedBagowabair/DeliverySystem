using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations.Validators
{
    public class UserValidator : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.Property(user => user.Email)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.HasIndex(user => user.Email).IsUnique();

            builder.Property(user => user.PasswordHash)
                   .IsRequired()
                   .HasMaxLength(512);

            builder.Property(user => user.Role)
                   .IsRequired()
                   .HasMaxLength(20);

            builder.Property(user => user.Permissions)
                   .HasMaxLength(500)
                   .HasDefaultValue(string.Empty);
        }
    }
}
