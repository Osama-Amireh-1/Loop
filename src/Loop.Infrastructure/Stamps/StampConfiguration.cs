using Domain.Stamps;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Stamps;

internal sealed class StampConfiguration : IEntityTypeConfiguration<Stamp>
{
    public void Configure(EntityTypeBuilder<Stamp> builder)
    {
        builder.HasKey(s => s.StampId);

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(s => s.Description)
            .HasColumnType("text");

        builder.Property(s => s.ImageUrl)
            .HasMaxLength(512);

        builder.Property(s => s.StampIconUrl)
            .HasMaxLength(512);

        builder.Property(s => s.StampsRequired)
            .IsRequired();

        builder.Property(s => s.RewardType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(s => s.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(s => s.StartDate)
            .IsRequired();
        builder.Property(s => s.EndDate).IsRequired();
        builder.Property(s => s.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("now()");

        builder.HasOne(s => s.Shop)
            .WithMany()
            .HasForeignKey(s => s.ShopId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(s => s.ShopId);
        builder.HasIndex(s => s.IsActive);
    }
}
