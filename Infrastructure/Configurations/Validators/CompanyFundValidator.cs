using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations.Validators
{
    public class CompanyFundValidator : IEntityTypeConfiguration<CompanyFund>
    {
        public void Configure(EntityTypeBuilder<CompanyFund> builder)
        {
            builder.ToTable("CompanyFunds");

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.Description)
                .HasMaxLength(500);

            builder.Property(x => x.Share)
                .IsRequired()
                .HasColumnType("decimal(7, 6)");

            builder.HasIndex(x => x.Name);

            builder.HasOne(x => x.CreatedByUser)
                .WithMany()
                .HasForeignKey(x => x.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.UpdatedByUser)
                .WithMany()
                .HasForeignKey(x => x.UpdatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class CompanyFundChangeLogValidator : IEntityTypeConfiguration<CompanyFundChangeLog>
    {
        public void Configure(EntityTypeBuilder<CompanyFundChangeLog> builder)
        {
            builder.ToTable("CompanyFundChangeLogs");

            builder.Property(x => x.FundName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.Action)
                .IsRequired()
                .HasMaxLength(30);

            builder.Property(x => x.OldShare)
                .HasColumnType("decimal(7, 6)");

            builder.Property(x => x.NewShare)
                .HasColumnType("decimal(7, 6)");

            builder.Property(x => x.Details)
                .HasMaxLength(2000);

            builder.HasIndex(x => x.ChangedAt);

            builder.HasOne(x => x.ChangedByUser)
                .WithMany()
                .HasForeignKey(x => x.ChangedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
