using Domain.Tiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Tiers;

internal sealed class TierConfiguration : IEntityTypeConfiguration<Tier>
{
    public void Configure(EntityTypeBuilder<Tier> builder)
    {
        builder.HasKey(t => t.TierId);

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(t => t.PointsRequired)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(t => t.Benefits)
            .HasColumnType("jsonb");

        builder.Property(t => t.IconUrl)
            .HasMaxLength(512);

        builder.Property(t => t.ColorHex)
            .HasMaxLength(7);

        builder.Property(t => t.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("now()");

        builder.Property(t => t.TierOrder)
            .IsRequired();

        builder.HasIndex(t => t.TierOrder).IsUnique();
        builder.HasIndex(t => t.Name).IsUnique();
    }
}
