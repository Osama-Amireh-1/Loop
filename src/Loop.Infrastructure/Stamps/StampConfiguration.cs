using System;
using Loop.Domain.Stamps;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Loop.Infrastructure.Stamps;

internal sealed class StampConfiguration : IEntityTypeConfiguration<Stamp>
{
    private static readonly Guid SeedShopId = Guid.Parse("7d5dc255-7f80-4f6f-b962-b83f0d0ac001");
    private static readonly Guid SeedStampId = Guid.Parse("3f3f3f15-0d2e-4ef6-8e55-8ebf30c81001");

    public void Configure(EntityTypeBuilder<Stamp> builder)
    {
        builder.HasKey(s => s.StampId);

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(s => s.Description)
                        .IsRequired()
            .HasColumnType("text")
            .IsRequired();

        builder.Property(s => s.ImageUrl).IsRequired()  
            .HasMaxLength(512)
            .IsRequired();

        builder.Property(s => s.StampIconUrl).IsRequired()
            .HasMaxLength(512)
            .IsRequired();
     

        builder.Property(s => s.StampsRequired)
            .IsRequired();

        builder.Property(s => s.RewardType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(s => s.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(s => s.StartDate)
            .IsRequired();
        builder.Property(s => s.EndDate)
            .IsRequired();
        builder.Property(s => s.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("now()");

        builder.HasOne(s => s.Shop)
            .WithMany()
            .HasForeignKey(s => s.ShopId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(s => s.ShopId);
        builder.HasIndex(s => s.IsActive);

        builder.HasData(new
        {
            StampId = SeedStampId,
            ShopId = SeedShopId,
            Name = "Buy 9 Get 1 Free",
            Description = "Collect 9 stamps and get your next drink free.",
            ImageUrl = "https://cdn.loop.local/stamps/loop-coffee-card.png",
            StampIconUrl = "https://cdn.loop.local/stamps/loop-coffee-icon.png",
            StampsRequired = 10,
            RewardType = StampType.Reward,
            IsActive = true,
            StartDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            EndDate = new DateTime(2027, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });
    }
}
