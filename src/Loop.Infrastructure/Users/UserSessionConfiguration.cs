using Loop.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Loop.Infrastructure.Users;

internal sealed class UserSessionConfiguration : IEntityTypeConfiguration<UserSession>
{
    public void Configure(EntityTypeBuilder<UserSession> builder)
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

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(session => session.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(session => session.UserId);
        builder.HasIndex(session => session.RefreshTokenHash).IsUnique();
    }
}

