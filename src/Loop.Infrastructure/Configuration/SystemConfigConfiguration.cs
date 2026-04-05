using Domain.Configuration;
using Domain.Malls;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configuration;

internal sealed class SystemConfigConfiguration : IEntityTypeConfiguration<SystemConfig>
{
    public void Configure(EntityTypeBuilder<SystemConfig> builder)
    {
        builder.HasKey(sc => sc.ConfigId);

        builder.Property(sc => sc.PointsToCurrencyRatio)
            .IsRequired()
            .HasPrecision(10, 4);

        builder.Property(sc => sc.EarnPointsPerCurrency)
            .IsRequired()
            .HasPrecision(10, 2);

        builder.Property(sc => sc.MinRedemptionThreshold)
            .IsRequired();

        builder.Property(sc => sc.UpdatedAt)
            .IsRequired()
            .HasDefaultValueSql("now()");

        builder.Property(sc => sc.UpdatedByAdminId)
            .IsRequired();

        builder.HasOne<MallAdmin>()
            .WithMany()
            .HasForeignKey(sc => sc.UpdatedByAdminId)
            .OnDelete(DeleteBehavior.Restrict);


        builder.HasOne<Mall>()
    .WithOne()
    .HasForeignKey<SystemConfig>(sc => sc.MallId)
    .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(sc => sc.MallId).IsUnique();
    }
}
