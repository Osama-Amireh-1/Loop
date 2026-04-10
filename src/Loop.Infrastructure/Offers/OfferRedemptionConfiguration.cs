using Loop.Domain.Offers;
using Loop.Domain.Shops;
using Loop.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Loop.Infrastructure.Offers;

internal sealed class OfferRedemptionConfiguration : IEntityTypeConfiguration<OfferRedemption>
{
    public void Configure(EntityTypeBuilder<OfferRedemption> builder)
    {
        builder.HasKey(or => or.RedemptionId);

        builder.Property(or => or.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("now()");

        builder.Property(or => or.RedemptionRef)
            .IsRequired(false);

        builder.HasOne<Shop>()
        .WithMany()
        .HasForeignKey(or => or.ShopId)
        .OnDelete(DeleteBehavior.Restrict);

     builder.HasOne<User>()
    .WithMany()
    .HasForeignKey(or => or.UserId)
    .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(or => or.OfferId);  
        builder.HasIndex(or => or.UserId);
        builder.HasIndex(or => new { or.UserId, or.OfferId });
    }
}

