using Domain.Receipts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Receipts;

internal sealed class ReceiptConfiguration : IEntityTypeConfiguration<Receipt>
{
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
        });
        builder.HasIndex(r => r.UserId);
        builder.HasIndex(r => r.ShopId);
        builder.HasIndex(r => r.Status);
    }
}
