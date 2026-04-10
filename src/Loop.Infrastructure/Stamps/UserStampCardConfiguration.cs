using System;
using Loop.Domain.Stamps;
using Loop.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Loop.Infrastructure.Stamps;

internal sealed class UserStampCardConfiguration : IEntityTypeConfiguration<UserStampCard>
{
    private static readonly Guid SeedUserId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid SeedStampId = Guid.Parse("3f3f3f15-0d2e-4ef6-8e55-8ebf30c81001");

    public void Configure(EntityTypeBuilder<UserStampCard> builder)
    {
        builder.HasKey(usc => new { usc.UserId, usc.StampId });

        builder.Property(usc => usc.StampsCounter)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(usc => usc.IsCompleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(usc => usc.LastTransaction)
            .IsRequired();

        builder.Property(usc => usc.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("now()");
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(usc => usc.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Stamp>(usc => usc.Stamp)
            .WithMany()
            .HasForeignKey(usc => usc.StampId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(usc => usc.UserId);
        builder.HasIndex(usc => usc.StampId);

        builder.HasData(new
        {
            UserId = SeedUserId,
            StampId = SeedStampId,
            StampsCounter = 4,
            IsCompleted = false,
            LastTransaction = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc),
            CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });
    }
}

