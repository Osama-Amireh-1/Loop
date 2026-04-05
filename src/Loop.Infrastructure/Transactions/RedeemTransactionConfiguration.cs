using Domain.Transactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Transactions;

internal sealed class RedeemTransactionConfiguration : IEntityTypeConfiguration<RedeemTransaction>
{
    public void Configure(EntityTypeBuilder<RedeemTransaction> builder)
    {
        builder.HasKey(r => r.RedeemId);

        builder.Property(r => r.PointsUsed)
            .IsRequired();

        builder.OwnsOne(r => r.DiscountValue, money =>
        {
            money.Property(m => m.Amount)
                .IsRequired()
                .HasPrecision(10, 2)
                .HasColumnName("DiscountAmount");

            money.Property(m => m.Currency)
                .IsRequired()
                .HasMaxLength(3)
                .HasColumnName("DiscountCurrency");
        });


        builder.Property(r => r.VerificationCode)
            .IsRequired()
            .HasMaxLength(6);

        builder.Property(r => r.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(r => r.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("now()");

        builder.Property(r => r.CompletedAt);

        builder.HasOne(r => r.User)
            .WithMany()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Shop)
            .WithMany()
            .HasForeignKey(r => r.ShopId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(r => r.UserId);
        builder.HasIndex(r => r.ShopId);
        builder.HasIndex(r => r.Status);
        builder.HasIndex(r => r.VerificationCode).IsUnique();
    }
}
