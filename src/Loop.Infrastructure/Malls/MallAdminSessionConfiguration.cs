using Loop.Domain.Malls;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Loop.Infrastructure.Malls;

internal sealed class MallAdminSessionConfiguration : IEntityTypeConfiguration<MallAdminSession>
{
    public void Configure(EntityTypeBuilder<MallAdminSession> builder)
    {
        builder.HasKey(session => session.SessionId);

        builder.Property(session => session.RefreshTokenHash)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(session => session.ExpiresAtUtc)
            .IsRequired();

        builder.Property(session => session.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("now()");

        builder.HasOne<MallAdmin>()
            .WithMany()
            .HasForeignKey(session => session.MallAdminId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(session => session.MallAdminId);
        builder.HasIndex(session => session.RefreshTokenHash).IsUnique();
    }
}
