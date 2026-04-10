using Loop.Domain.Audit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Loop.Infrastructure.Audit;

internal sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.HasKey(a => a.LogId);

        builder.Property(a => a.AdminType)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(a => a.ActionType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.Metadata)
            .HasColumnType("jsonb");

        builder.Property(a => a.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("now()");

        builder.HasOne(a => a.User)
            .WithMany()
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(a => a.Shop)
            .WithMany()
            .HasForeignKey(a => a.ShopId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(a => a.ShopAdmin)
            .WithMany()
            .HasForeignKey(a => a.ShopAdminId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(a => a.MallAdmin)
            .WithMany()
            .HasForeignKey(a => a.MallAdminId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Property(a => a.Points);

        builder.HasIndex(a => a.UserId);
        builder.HasIndex(a => a.ShopId);
        builder.HasIndex(a => a.CreatedAt);
    }
}

