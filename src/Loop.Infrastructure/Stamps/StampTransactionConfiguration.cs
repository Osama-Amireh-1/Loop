using Loop.Domain.Stamps;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Loop.Infrastructure.Stamps;

internal sealed class StampTransactionConfiguration : IEntityTypeConfiguration<StampTransaction>
{
    public void Configure(EntityTypeBuilder<StampTransaction> builder)
    {
        builder.HasKey(st => st.StampTxId);

        builder.Property(st => st.Type)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(st => st.StampsCount)
            .IsRequired();

        builder.Property(st => st.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("now()");

        builder.HasOne(st => st.User)
            .WithMany()
            .HasForeignKey(st => st.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(st => st.Shop)
            .WithMany()
            .HasForeignKey(st => st.ShopId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(st => st.StampProgramId)
            .IsRequired();

        builder.Property(st => st.RedemptionRef)
            .IsRequired(false);
        builder.HasOne<Stamp>()
    .WithMany()
    .HasForeignKey(st => st.StampProgramId)
    .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(st => st.UserId);
        builder.HasIndex(st => st.ShopId);
        builder.HasIndex(st => st.StampProgramId);
    }
}

