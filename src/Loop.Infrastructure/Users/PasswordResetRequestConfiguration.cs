using Loop.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Loop.Infrastructure.Users;

internal sealed class PasswordResetRequestConfiguration : IEntityTypeConfiguration<PasswordResetRequest>
{
    public void Configure(EntityTypeBuilder<PasswordResetRequest> builder)
    {
        builder.HasKey(request => request.RequestId);

        builder.Property(request => request.TokenHash)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(request => request.ExpiresAtUtc)
            .IsRequired();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(request => request.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(request => request.UserId).IsUnique();
        builder.HasIndex(request => request.TokenHash).IsUnique();
    }
}

