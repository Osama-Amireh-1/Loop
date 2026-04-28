using Loop.Domain.Shops;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Loop.Infrastructure.Shops;

internal sealed class ShopPointsWalletConfiguration : IEntityTypeConfiguration<ShopPointsWallet>
{
    public void Configure(EntityTypeBuilder<ShopPointsWallet> builder)
    {
        builder.ToTable("shop_points_wallet");

        builder.HasKey(x => x.ShopId);

        builder.Property(x => x.ShopId)
            .ValueGeneratedNever();

        builder.Property(x => x.PointsReceived)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(x => x.LastUpdated)
            .IsRequired()
            .HasDefaultValueSql("now()");

        builder.HasOne(x => x.Shop)
            .WithOne(s => s.PointsWallet)
            .HasForeignKey<ShopPointsWallet>(x => x.ShopId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
