using Loop.Domain.Shops;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Loop.Infrastructure.Shops;

internal sealed class ShopAdminSessionConfiguration : IEntityTypeConfiguration<ShopAdminSession>
{
    public void Configure(EntityTypeBuilder<ShopAdminSession> builder)
    {
        builder.HasKey(session => session.SessionId);

        builder.Property(session => session.RefreshTokenHash)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(session => session.ExpiresAtUtc)
            .IsRequired();

        builder.Property(session => session.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("now()");

        builder.HasOne<ShopAdmin>()
            .WithMany()
            .HasForeignKey(session => session.ShopAdminId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(session => session.ShopAdminId);
        builder.HasIndex(session => session.RefreshTokenHash).IsUnique();
    }
}
