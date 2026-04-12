using System;
using Loop.Domain.Offers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Loop.Infrastructure.Offers;

internal sealed class OfferConfiguration : IEntityTypeConfiguration<Offer>
{
    private static readonly Guid SeedShopLoopCoffeeId = Guid.Parse("7d5dc255-7f80-4f6f-b962-b83f0d0ac001");
    private static readonly Guid SeedShopUrbanWearId = Guid.Parse("7d5dc255-7f80-4f6f-b962-b83f0d0ac002");
    private static readonly Guid SeedShopTechZoneId = Guid.Parse("7d5dc255-7f80-4f6f-b962-b83f0d0ac003");

    private static readonly Guid SeedOfferCoffeeId = Guid.Parse("77777777-7777-7777-7777-777777777777");
    private static readonly Guid SeedOfferFashionId = Guid.Parse("77777777-7777-7777-7777-777777777778");
    private static readonly Guid SeedOfferElectronicsId = Guid.Parse("77777777-7777-7777-7777-777777777779");

    public void Configure(EntityTypeBuilder<Offer> builder)
    {
        builder.HasKey(o => o.OfferId);

        builder.Property(o => o.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(o => o.Description)
            .HasColumnType("text")
               .IsRequired();

        builder.Property(o => o.ImageUrl)
            .HasMaxLength(512)
                        .IsRequired();

        builder.Property(o => o.RewardType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(o => o.RewardValue)
            .HasColumnType("jsonb")
                        .IsRequired();

        builder.Property(o => o.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(o => o.StartDate)
            .IsRequired();

        builder.Property(o => o.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("now()");

        builder.Property(o => o.EndDate)
            .IsRequired();

        builder.HasOne(o => o.Shop)
            .WithMany()
            .HasForeignKey(o => o.ShopId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(o => o.Redemptions)
            .WithOne()
            .HasForeignKey(or => or.OfferId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(o => o.ShopId);
        builder.HasIndex(o => o.IsActive);
        builder.HasIndex(o => o.StartDate);

        builder.HasData(
            new
            {
                OfferId = SeedOfferCoffeeId,
                ShopId = SeedShopLoopCoffeeId,
                Name = "10% Off Drinks",
                Description = "Get 10% discount on any drink.",
                ImageUrl = "https://cdn.loop.local/offers/ten-percent.png",
                RewardType = RewardType.Discount,
                RewardValue = "{\"percent\":10}",
                IsActive = true,
                StartDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                EndDate = new DateTime(2027, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new
            {
                OfferId = SeedOfferFashionId,
                ShopId = SeedShopUrbanWearId,
                Name = "Buy 2 Get 1",
                Description = "Buy 2 items and get 1 free.",
                ImageUrl = "https://cdn.loop.local/offers/buy2get1.png",
                RewardType = RewardType.Discount,
                RewardValue = "{\"type\":\"buy2get1\"}",
                IsActive = true,
                StartDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                EndDate = new DateTime(2027, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new
            {
                OfferId = SeedOfferElectronicsId,
                ShopId = SeedShopTechZoneId,
                Name = "Free Accessory",
                Description = "Get a free accessory with selected devices.",
                ImageUrl = "https://cdn.loop.local/offers/free-accessory.png",
                RewardType = RewardType.Discount,
                RewardValue = "{\"value\":\"free_accessory\"}",
                IsActive = true,
                StartDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                EndDate = new DateTime(2027, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            });
    }
}


