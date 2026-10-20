using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations.Validators
{
    public class BusinessDayStateValidator : IEntityTypeConfiguration<BusinessDayState>
    {
        public void Configure(EntityTypeBuilder<BusinessDayState> builder)
        {
            builder.ToTable("BusinessDayStates");

            builder.Property(x => x.CurrentBusinessDate)
                .IsRequired()
                .HasColumnType("date");

            builder.HasOne(x => x.UpdatedByUser)
                .WithMany()
                .HasForeignKey(x => x.UpdatedByUserId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);
        }
    }
}
