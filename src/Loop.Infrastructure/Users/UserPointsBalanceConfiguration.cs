using System;
using Loop.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Loop.Infrastructure.Users;

internal sealed class UserPointsBalanceConfiguration : IEntityTypeConfiguration<UserPointsBalance>
{
    private static readonly Guid SeedUserId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid SeedUserPointsBalanceId = Guid.Parse("33333333-3333-3333-3333-333333333333");

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

        builder.HasData(new
        {
            UserPointsBalanceId = SeedUserPointsBalanceId,
            UserId = SeedUserId,
            TotalPoints = 350,
            LifetimePoints = 500,
            LastUpdated = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });
    }
}

