using Loop.Domain.Transactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Loop.Infrastructure.Transactions;

internal sealed class EarnTransactionConfiguration : IEntityTypeConfiguration<EarnTransaction>
{
    public void Configure(EntityTypeBuilder<EarnTransaction> builder)
    {
        builder.HasKey(e => e.EarnId);

        builder.Property(e => e.PointsEarned)
            .IsRequired();

        builder.Property(e => e.TransactionRef)
            .HasMaxLength(200);

        builder.Property(e => e.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("now()");

        builder.OwnsOne(e => e.PurchaseAmount, money =>
        {
            money.Property(m => m.Amount)
                .IsRequired()
                .HasPrecision(10, 2)
                .HasColumnName("PurchaseAmount");

            money.Property(m => m.Currency)
                .IsRequired()
                .HasMaxLength(3)
                .HasColumnName("PurchaseCurrency");
        });

        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Shop)
            .WithMany()
            .HasForeignKey(e => e.ShopId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.UserId);
        builder.HasIndex(e => e.ShopId);
        builder.HasIndex(e => e.CreatedAt);
    }
}

