using Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Users;

internal sealed class UserPointsBalanceConfiguration : IEntityTypeConfiguration<UserPointsBalance>
{
    public void Configure(EntityTypeBuilder<UserPointsBalance> builder)
    {
        builder.HasKey(upb => upb.UserPointsBalanceId);

        builder.Property(upb => upb.TotalPoints)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(upb => upb.LifetimePoints)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(upb => upb.LastUpdated)
            .IsRequired()
            .HasDefaultValueSql("now()");
    }
}
