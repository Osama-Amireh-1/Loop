using Domain.Malls;
using Domain.Shops;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Shops;

internal sealed class ShopConfiguration : IEntityTypeConfiguration<Shop>
{
    public void Configure(EntityTypeBuilder<Shop> builder)
    {
        builder.HasKey(s => s.ShopId);

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(s => s.LogoUrl)
            .HasMaxLength(512);

        builder.Property(s => s.CoverImageUrl)
            .HasMaxLength(512);

        builder.Property(s => s.Bio)
            .HasColumnType("text");

        builder.Property(s => s.WebsiteUrl)
            .HasMaxLength(512);

        builder.Property(s => s.SocialLinks)
            .HasColumnType("jsonb");

        builder.Property(s => s.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(s => s.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("now()");

        builder.HasIndex(s => s.MallId);
        builder.HasIndex(s => s.CategoryId);
        builder.HasIndex(s => s.Name).IsUnique();

        builder.HasOne(s => s.Category)
            .WithMany(c => c.Shops)
            .HasForeignKey(s => s.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Mall>()
    .WithMany()
    .HasForeignKey(s => s.MallId)
    .OnDelete(DeleteBehavior.Restrict);
    }
}
