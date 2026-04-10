using System;
using Loop.Domain.Configuration;
using Loop.Domain.Malls;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Loop.Infrastructure.Malls;

internal sealed class MallConfiguration : IEntityTypeConfiguration<Mall>
{
    private static readonly Guid SeedMallId = Guid.Parse("d86d8ee3-4bf9-47a8-b2c2-81a6f09bf001");

    public void Configure(EntityTypeBuilder<Mall> builder)
    {
        builder.HasKey(m => m.MallId);

        builder.Property(m => m.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(m => m.Location)
            .HasMaxLength(500);

        builder.Property(m => m.LogoUrl)
            .HasMaxLength(512);

        builder.Property(m => m.CoverImageUrl)
            .HasMaxLength(512);

        builder.Property(m => m.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("now()");

        builder.HasOne(m => m.SystemConfig)
            .WithOne()
            .HasForeignKey<SystemConfig>(sc => sc.MallId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(m => m.Name).IsUnique();

        builder.HasData(new
        {
            MallId = SeedMallId,
            Name = "Loop Central Mall",
            Location = "Amman, Jordan",
            LogoUrl = "https://cdn.loop.local/malls/loop-central-logo.png",
            CoverImageUrl = "https://cdn.loop.local/malls/loop-central-cover.png",
            CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });
    }
}

