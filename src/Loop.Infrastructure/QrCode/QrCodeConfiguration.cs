using Domain.QRCode;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Loop.Infrastructure.QRCode;

internal sealed class QrCodeConfiguration : IEntityTypeConfiguration<QrCode>
{
    public void Configure(EntityTypeBuilder<QrCode> builder)
    {
        builder.HasKey(q => q.QrId);

        builder.Property(q => q.QrCodeData)
            .IsRequired()
            .HasColumnType("jsonb");

        builder.Property(q => q.ExpiresAt)
            .IsRequired();

        builder.HasOne(q => q.User)
            .WithMany()
            .HasForeignKey(q => q.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(q => q.Shop)
    .WithMany()
    .HasForeignKey(q => q.ShopId)
    .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(q => q.UserId);
        builder.HasIndex(q => q.ExpiresAt);
    }
}
