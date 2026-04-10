using System;
using Loop.Domain.Configuration;
using Loop.Domain.Malls;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Loop.Infrastructure.Configuration;

internal sealed class SystemConfigConfiguration : IEntityTypeConfiguration<SystemConfig>
{
    private static readonly Guid SeedConfigId = Guid.Parse("66666666-6666-6666-6666-666666666666");
    private static readonly Guid SeedMallId = Guid.Parse("d86d8ee3-4bf9-47a8-b2c2-81a6f09bf001");
    private static readonly Guid SeedMallAdminId = Guid.Parse("55555555-5555-5555-5555-555555555555");

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

        builder.HasData(new
        {
            ConfigId = SeedConfigId,
            MallId = SeedMallId,
            PointsToCurrencyRatio = 0.0100m,
            EarnPointsPerCurrency = 1.00m,
            MinRedemptionThreshold = 100,
            UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            UpdatedByAdminId = SeedMallAdminId
        });
    }
}

