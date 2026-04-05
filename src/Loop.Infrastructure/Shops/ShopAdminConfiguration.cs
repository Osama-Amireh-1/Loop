using Domain.Common;
using Domain.Shops;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Shops;

internal sealed class ShopAdminConfiguration : IEntityTypeConfiguration<ShopAdmin>
{
    public void Configure(EntityTypeBuilder<ShopAdmin> builder)
    {
        builder.HasKey(sa => sa.ShopAdminId);

        builder.Property(sa => sa.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(sa => sa.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(sa => sa.Phone)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(sa => sa.PasswordHash)
            .IsRequired();

        builder.Property(sa => sa.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(sa => sa.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("now()");

        builder.Property(sa => sa.ShopId).IsRequired();

        builder.HasOne<Shop>()
            .WithMany()
            .HasForeignKey(sa => sa.ShopId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(sa => sa.Email)
    .IsRequired()
    .HasMaxLength(256)
    .HasConversion(e => e.Value, v => Email.Create(v));

        builder.Property(sa => sa.Phone)
           .IsRequired()
           .HasMaxLength(20)
           .HasConversion(p => p.Value, v => Phone.Create(v));

        builder.HasIndex(sa => sa.Email).IsUnique();
        builder.HasIndex(sa => sa.Phone).IsUnique();
    }
}
