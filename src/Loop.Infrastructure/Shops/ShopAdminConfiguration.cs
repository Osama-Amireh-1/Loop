using System;
using Loop.Domain.Common;
using Loop.Domain.Shops;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Loop.Infrastructure.Shops;

internal sealed class ShopAdminConfiguration : IEntityTypeConfiguration<ShopAdmin>
{

    public void Configure(EntityTypeBuilder<ShopAdmin> builder)
    {
        builder.HasKey(sa => sa.ShopAdminId);

        builder.Property(sa => sa.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.OwnsOne(sa => sa.Email, email =>
        {
            email.Property(e => e.Value)
                .IsRequired()
                .HasMaxLength(256)
                .HasColumnName("Email");

            email.HasIndex(e => e.Value).IsUnique();

           
        });

        builder.Property(sa => sa.Phone)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion(p => p.Value, v => Phone.Create(v).Value);

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
        builder.HasIndex(sa => sa.Phone).IsUnique();


    }
}


