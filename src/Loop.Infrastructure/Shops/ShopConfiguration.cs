using System;
using Loop.Domain.Malls;
using Loop.Domain.Shops;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Loop.Infrastructure.Shops;

internal sealed class ShopConfiguration : IEntityTypeConfiguration<Shop>
{
    private static readonly Guid SeedShopId = Guid.Parse("7d5dc255-7f80-4f6f-b962-b83f0d0ac001");
    private static readonly Guid SeedMallId = Guid.Parse("d86d8ee3-4bf9-47a8-b2c2-81a6f09bf001");
    private static readonly Guid SeedCategoryId = Guid.Parse("9f6454d5-a767-4c2f-a38d-cf6c9ee1ec01");

    public void Configure(EntityTypeBuilder<Shop> builder)
    {
        builder.HasKey(s => s.ShopId);

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(s => s.LogoUrl)
                        .IsRequired()
            .HasMaxLength(512);

        builder.Property(s => s.CoverImageUrl)
                        .IsRequired()
            .HasMaxLength(512);

        builder.Property(s => s.Bio)
                        .IsRequired()
            .HasColumnType("text");

        builder.Property(s => s.WebsiteUrl)
            .HasMaxLength(512);

        builder.Property(s => s.SocialLinks)
            .HasColumnType("jsonb");

        builder.Property(s => s.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(s => s.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("now()");

        builder.HasIndex(s => s.MallId);
        builder.HasIndex(s => s.CategoryId);
        builder.HasIndex(s => s.Name).IsUnique();

        builder.HasOne(s => s.Category)
            .WithMany(c => c.Shops)
            .HasForeignKey(s => s.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Mall>()
    .WithMany()
    .HasForeignKey(s => s.MallId)
    .OnDelete(DeleteBehavior.Restrict);

        builder.HasData(new
        {
            ShopId = SeedShopId,
            MallId = SeedMallId,
            Name = "Loop Coffee",
            CategoryId = SeedCategoryId,
            LogoUrl = "https://cdn.loop.local/shops/loop-coffee-logo.png",
            CoverImageUrl = "https://cdn.loop.local/shops/loop-coffee-cover.png",
            Bio = "Specialty coffee and pastries.",
            WebsiteUrl = "https://loop-coffee.local",
            SocialLinks = "{\"instagram\":\"@loopcoffee\"}",
            IsActive = true,
            CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });
    }
}

