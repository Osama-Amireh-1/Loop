using Loop.Domain.QRCode;
using Loop.Domain.Shops;
using Loop.Domain.Stamps;
using Loop.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Loop.Infrastructure.Stamps;

internal sealed class StampRedemptionConfiguration : IEntityTypeConfiguration<StampRedemption>
{
    public void Configure(EntityTypeBuilder<StampRedemption> builder)
    {
        builder.HasKey(sr => sr.RedemptionId);

        builder.Property(sr => sr.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("now()");

        builder.Property(sr => sr.RedemptionRef)
            .IsRequired(false);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(sr => sr.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Shop>()
            .WithMany()
            .HasForeignKey(sr => sr.ShopId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Stamp>()
            .WithMany()
            .HasForeignKey(sr => sr.StampId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<QrCode>()
            .WithMany()
            .HasForeignKey(sr => sr.RedemptionRef)
            .HasPrincipalKey(q => q.QrId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(sr => sr.UserId);
        builder.HasIndex(sr => sr.ShopId);
        builder.HasIndex(sr => sr.StampId);
        builder.HasIndex(sr => new { sr.UserId, sr.StampId });
        builder.HasIndex(sr => sr.RedemptionRef).IsUnique();
    }
}
