using System;
using Loop.Domain.Malls;
using Loop.Domain.Shops;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Loop.Infrastructure.Shops;

internal sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    private static readonly Guid SeedCategoryCoffeeId = Guid.Parse("9f6454d5-a767-4c2f-a38d-cf6c9ee1ec01");
    private static readonly Guid SeedCategoryFashionId = Guid.Parse("9f6454d5-a767-4c2f-a38d-cf6c9ee1ec02");
    private static readonly Guid SeedCategoryElectronicsId = Guid.Parse("9f6454d5-a767-4c2f-a38d-cf6c9ee1ec03");
    private static readonly Guid SeedMallId = Guid.Parse("d86d8ee3-4bf9-47a8-b2c2-81a6f09bf001");

    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.HasKey(c => c.CategoryId);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.IconUrl)
            .HasMaxLength(512)
                        .IsRequired();

        builder.Property(c => c.DisplayOrder)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(c => c.Description)
            .HasColumnType("text")
                        .IsRequired();

        builder.Property(c => c.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("now()");

        builder.Property(c => c.MallId)
            .IsRequired();

        builder.HasOne<Mall>()
            .WithMany()
            .HasForeignKey(c => c.MallId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(c => c.Name).IsUnique();

        builder.HasData(
            new
            {
                CategoryId = SeedCategoryCoffeeId,
                MallId = SeedMallId,
                Name = "Coffee",
                IconUrl = "https://cdn.loop.local/icons/coffee.png",
                DisplayOrder = 1,
                Description = "Coffee and beverages",
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new
            {
                CategoryId = SeedCategoryFashionId,
                MallId = SeedMallId,
                Name = "Fashion",
                IconUrl = "https://cdn.loop.local/icons/fashion.png",
                DisplayOrder = 2,
                Description = "Clothing, shoes and accessories",
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new
            {
                CategoryId = SeedCategoryElectronicsId,
                MallId = SeedMallId,
                Name = "Electronics",
                IconUrl = "https://cdn.loop.local/icons/electronics.png",
                DisplayOrder = 3,
                Description = "Phones, gadgets and accessories",
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            });
    }
}


