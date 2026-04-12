using Loop.Domain.Malls;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Loop.Infrastructure.Malls;

internal sealed class MallAdminPasswordResetRequestConfiguration : IEntityTypeConfiguration<MallAdminPasswordResetRequest>
{
    public void Configure(EntityTypeBuilder<MallAdminPasswordResetRequest> builder)
    {
        builder.HasKey(request => request.RequestId);

        builder.Property(request => request.TokenHash)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(request => request.ExpiresAtUtc)
            .IsRequired();

        builder.HasOne<MallAdmin>()
            .WithMany()
            .HasForeignKey(request => request.MallAdminId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(request => request.MallAdminId).IsUnique();
        builder.HasIndex(request => request.TokenHash).IsUnique();
    }
}
