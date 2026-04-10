using System;
using Loop.Domain.Common;
using Loop.Domain.Tiers;
using Loop.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Loop.Infrastructure.Users;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    private static readonly Guid SeedTierId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid SeedUserId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.UserId);

        builder.Property(u => u.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.Phone)
            .HasConversion(p => p.Value, value => Phone.Create(value))
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(u => u.Email)
            .HasConversion(e => e.Value, value => Email.Create(value))
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(u => u.PasswordHash)
            .IsRequired();
        builder.Property(u => u.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("now()");

        builder.Property(u => u.Gender)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(10);

        builder.Property(u => u.ProfileImageUrl)
            .HasMaxLength(512);

        builder.HasOne<Tier>()
            .WithMany()
            .HasForeignKey(u => u.TierId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(u => u.Email).IsUnique();
        builder.HasIndex(u => u.Phone).IsUnique();
        builder.HasIndex(u => u.TierId);

        builder.HasOne(u => u.PointsBalance)
            .WithOne()
            .HasForeignKey<UserPointsBalance>(pb => pb.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasData(new
        {
            UserId = SeedUserId,
            FirstName = "Demo",
            LastName = "User",
            Phone = Phone.Create("+962790000001"),
            Email = Email.Create("demo.user@loop.local"),
            PasswordHash = "seeded-password-hash",
            Gender = Gender.Male,
            ProfileImageUrl = "https://cdn.loop.local/users/demo-user.png",
            TierId = SeedTierId,
            CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });
    }
}

