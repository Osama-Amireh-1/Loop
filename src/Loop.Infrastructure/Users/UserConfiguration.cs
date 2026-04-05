using Domain.Common;
using Domain.Tiers;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Users;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
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
    }
}
