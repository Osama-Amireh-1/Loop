using System;
using Loop.Domain.Tiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Loop.Infrastructure.Tiers;

internal sealed class TierConfiguration : IEntityTypeConfiguration<Tier>
{
    private static readonly Guid SeedTierId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    public void Configure(EntityTypeBuilder<Tier> builder)
    {
        builder.HasKey(t => t.TierId);

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(t => t.PointsRequired)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(t => t.Benefits)
            .HasColumnType("jsonb");

        builder.Property(t => t.IconUrl)
            .IsRequired()
            .HasMaxLength(512);

        builder.Property(t => t.ColorHex)
            .HasMaxLength(7);

        builder.Property(t => t.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("now()");

        builder.Property(t => t.TierOrder)
            .IsRequired();

        builder.HasIndex(t => t.TierOrder).IsUnique();
        builder.HasIndex(t => t.Name).IsUnique();

        builder.HasData(new
        {
            TierId = SeedTierId,
            TierOrder = 1,
            Name = "Bronze",
            PointsRequired = 0,
            Benefits = "{\"multiplier\":1.0}",
            IconUrl = "https://cdn.loop.local/tiers/bronze.png",
            ColorHex = "#CD7F32",
            CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });
    }
}

