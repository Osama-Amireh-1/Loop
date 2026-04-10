using System;
using Loop.Domain.Common;
using Loop.Domain.Shops;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Loop.Infrastructure.Shops;

internal sealed class ShopAdminConfiguration : IEntityTypeConfiguration<ShopAdmin>
{
    private static readonly Guid SeedShopId = Guid.Parse("7d5dc255-7f80-4f6f-b962-b83f0d0ac001");
    private static readonly Guid SeedShopAdminId = Guid.Parse("44444444-4444-4444-4444-444444444444");

    public void Configure(EntityTypeBuilder<ShopAdmin> builder)
    {
        builder.HasKey(sa => sa.ShopAdminId);

        builder.Property(sa => sa.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(sa => sa.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(sa => sa.Phone)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(sa => sa.PasswordHash)
            .IsRequired();

        builder.Property(sa => sa.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(sa => sa.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("now()");

        builder.Property(sa => sa.ShopId).IsRequired();

        builder.HasOne<Shop>()
            .WithMany()
            .HasForeignKey(sa => sa.ShopId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(sa => sa.Email)
            .IsRequired()
            .HasMaxLength(256)
            .HasConversion(e => e.Value, v => Email.Create(v));

        builder.Property(sa => sa.Phone)
           .IsRequired()
           .HasMaxLength(20)
           .HasConversion(p => p.Value, v => Phone.Create(v));

        builder.HasIndex(sa => sa.Email).IsUnique();
        builder.HasIndex(sa => sa.Phone).IsUnique();

        builder.HasData(new
        {
            ShopAdminId = SeedShopAdminId,
            ShopId = SeedShopId,
            Name = "Loop Coffee Admin",
            Email = Email.Create("shop.admin@loop.local"),
            Phone = Phone.Create("+962790000002"),
            PasswordHash = "seeded-password-hash",
            IsActive = true,
            CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });
    }
}

