using Domain.Offers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Offers;

internal sealed class OfferConfiguration : IEntityTypeConfiguration<Offer>
{
    public void Configure(EntityTypeBuilder<Offer> builder)
    {
        builder.HasKey(o => o.OfferId);

        builder.Property(o => o.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(o => o.Description)
            .HasColumnType("text");

        builder.Property(o => o.ImageUrl)
            .HasMaxLength(512);

        builder.Property(o => o.RewardType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(o => o.RewardValue)
            .HasColumnType("jsonb");

        builder.Property(o => o.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(o => o.StartDate)
            .IsRequired();

        builder.Property(o => o.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("now()");

        builder.Property(o => o.EndDate)
                        .IsRequired();

        builder.HasOne(o => o.Shop)
            .WithMany()
            .HasForeignKey(o => o.ShopId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(o => o.Redemptions)
            .WithOne()
            .HasForeignKey(or => or.OfferId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(o => o.ShopId);
        builder.HasIndex(o => o.IsActive);
        builder.HasIndex(o => o.StartDate);
    }
}
