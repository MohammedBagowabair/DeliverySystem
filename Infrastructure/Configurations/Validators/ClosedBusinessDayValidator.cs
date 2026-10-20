using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations.Validators
{
    public class ClosedBusinessDayValidator : IEntityTypeConfiguration<ClosedBusinessDay>
    {
        public void Configure(EntityTypeBuilder<ClosedBusinessDay> builder)
        {
            builder.ToTable("ClosedBusinessDays");

            builder.Property(x => x.BusinessDate)
                .IsRequired()
                .HasColumnType("date");

            builder.HasIndex(x => x.BusinessDate)
                .IsUnique();

            builder.Property(x => x.Notes)
                .HasMaxLength(500);

            builder.Property(x => x.TotalRevenue)
                .HasColumnType("decimal(18, 2)");

            builder.HasOne(x => x.ClosedByUser)
                .WithMany()
                .HasForeignKey(x => x.ClosedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
