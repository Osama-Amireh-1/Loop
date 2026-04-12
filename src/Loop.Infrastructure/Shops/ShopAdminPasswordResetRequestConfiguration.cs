using Loop.Domain.Shops;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Loop.Infrastructure.Shops;

internal sealed class ShopAdminPasswordResetRequestConfiguration : IEntityTypeConfiguration<ShopAdminPasswordResetRequest>
{
    public void Configure(EntityTypeBuilder<ShopAdminPasswordResetRequest> builder)
    {
        builder.HasKey(request => request.RequestId);

        builder.Property(request => request.TokenHash)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(request => request.ExpiresAtUtc)
            .IsRequired();

        builder.HasOne<ShopAdmin>()
            .WithMany()
            .HasForeignKey(request => request.ShopAdminId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(request => request.ShopAdminId).IsUnique();
        builder.HasIndex(request => request.TokenHash).IsUnique();
    }
}
