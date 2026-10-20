using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations.Validators
{
    public class ClosedBusinessYearValidator : IEntityTypeConfiguration<ClosedBusinessYear>
    {
        public void Configure(EntityTypeBuilder<ClosedBusinessYear> builder)
        {
            builder.ToTable("ClosedBusinessYears");

            builder.Property(x => x.Year)
                .IsRequired();

            builder.HasIndex(x => x.Year)
                .IsUnique();

            builder.Property(x => x.Notes)
                .HasMaxLength(500);

            builder.Property(x => x.TotalRevenue)
                .HasColumnType("decimal(18, 2)");

            builder.Property(x => x.DriverProfit)
                .HasColumnType("decimal(18, 2)");

            builder.Property(x => x.FundSharesJson)
                .IsRequired()
                .HasColumnType("nvarchar(max)");

            builder.HasOne(x => x.ClosedByUser)
                .WithMany()
                .HasForeignKey(x => x.ClosedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
