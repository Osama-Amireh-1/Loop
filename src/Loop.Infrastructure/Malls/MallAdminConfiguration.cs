using System;
using Loop.Domain.Common;
using Loop.Domain.Malls;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Loop.Infrastructure.Malls;

internal sealed class MallAdminConfiguration : IEntityTypeConfiguration<MallAdmin>
{
    private static readonly Guid SeedMallId = Guid.Parse("d86d8ee3-4bf9-47a8-b2c2-81a6f09bf001");
    private static readonly Guid SeedMallAdminId = Guid.Parse("55555555-5555-5555-5555-555555555555");

    public void Configure(EntityTypeBuilder<MallAdmin> builder)
    {
        builder.HasKey(ma => ma.MallAdminId);

        builder.Property(ma => ma.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(sa => sa.Email)
            .IsRequired()
            .HasMaxLength(256)
            .HasConversion(e => e.Value, v => Email.Create(v));

        builder.Property(sa => sa.Phone)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion(p => p.Value, v => Phone.Create(v));

        builder.Property(ma => ma.PasswordHash)
            .IsRequired();

        builder.Property(ma => ma.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("now()");

        builder.Property(ma => ma.MallId)
            .IsRequired();

        builder.HasOne<Mall>()
            .WithMany()
            .HasForeignKey(x => x.MallId);

        builder.Property(sa => sa.Email)
            .IsRequired()
            .HasMaxLength(256)
            .HasConversion(e => e.Value, v => Email.Create(v));

        builder.Property(sa => sa.Phone)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion(p => p.Value, v => Phone.Create(v));

        builder.HasIndex(ma => ma.Email).IsUnique();
        builder.HasIndex(ma => ma.Phone).IsUnique();

        builder.HasData(new
        {
            MallAdminId = SeedMallAdminId,
            MallId = SeedMallId,
            Name = "Loop Mall Admin",
            Email = Email.Create("mall.admin@loop.local"),
            Phone = Phone.Create("+962790000003"),
            PasswordHash = "seeded-password-hash",
            CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });
    }
}

