using System;
using Loop.Domain.Receipts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Loop.Infrastructure.Receipts;

internal sealed class ReceiptConfiguration : IEntityTypeConfiguration<Receipt>
{
    private static readonly Guid SeedReceiptId = Guid.Parse("88888888-8888-8888-8888-888888888888");
    private static readonly Guid SeedShopId = Guid.Parse("7d5dc255-7f80-4f6f-b962-b83f0d0ac001");
    private static readonly Guid SeedUserId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    public void Configure(EntityTypeBuilder<Receipt> builder)
    {
        builder.HasKey(r => r.ReceiptId);

        builder.Property(r => r.ReceiptPath)
            .HasMaxLength(512);

        builder.Property(r => r.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(r => r.ReceiptDetails)
            .HasColumnType("jsonb");

        builder.HasOne(r => r.Shop)
            .WithMany()
            .HasForeignKey(r => r.ShopId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.User)
            .WithMany()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.OwnsOne(r => r.Amount, money =>
        {
            money.Property(m => m.Amount)
                .IsRequired()
                .HasPrecision(12, 2)
                .HasColumnName("Amount");

            money.Property(m => m.Currency)
                .IsRequired()
                .HasMaxLength(3)
                .HasColumnName("Currency");

            money.HasData(new
            {
                ReceiptId = SeedReceiptId,
                Amount = 12.50m,
                Currency = "JOD"
            });
        });
        builder.HasIndex(r => r.UserId);
        builder.HasIndex(r => r.ShopId);
        builder.HasIndex(r => r.Status);

        builder.HasData(new
        {
            ReceiptId = SeedReceiptId,
            ReceiptPath = "receipts/2026/seed-receipt-1.jpg",
            ShopId = SeedShopId,
            UserId = SeedUserId,
            ReceiptDetails = "{\"items\":1,\"source\":\"seed\"}",
            Status = ReceiptStatus.Approved
        });
    }
}

