using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations.Validators
{
    public class CompanySettingsValidator : IEntityTypeConfiguration<CompanySettings>
    {
        public void Configure(EntityTypeBuilder<CompanySettings> builder)
        {
            builder.Property(x => x.ExpensesShare)
                .IsRequired()
                .HasColumnType("decimal(5, 4)");

            builder.Property(x => x.ManagersShare)
                .IsRequired()
                .HasColumnType("decimal(5, 4)");

            builder.Property(x => x.ManagementShare)
                .IsRequired()
                .HasColumnType("decimal(5, 4)");

            builder.Property(x => x.CompanyReserveShare)
                .IsRequired()
                .HasColumnType("decimal(5, 4)");
        }
    }
}
