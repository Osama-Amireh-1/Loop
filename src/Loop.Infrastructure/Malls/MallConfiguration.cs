using Domain.Configuration;
using Domain.Malls;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Malls;

internal sealed class MallConfiguration : IEntityTypeConfiguration<Mall>
{
    public void Configure(EntityTypeBuilder<Mall> builder)
    {
        builder.HasKey(m => m.MallId);

        builder.Property(m => m.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(m => m.Location)
            .HasMaxLength(500);

        builder.Property(m => m.LogoUrl)
            .HasMaxLength(512);

        builder.Property(m => m.CoverImageUrl)
            .HasMaxLength(512);

        builder.Property(m => m.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("now()");
        builder.Property(m => m.SystemConfigId);

        builder.HasOne(m => m.SystemConfig)
            .WithOne()
            .HasForeignKey<SystemConfig>(sc => sc.MallId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(m => m.Name).IsUnique();
    }
}
