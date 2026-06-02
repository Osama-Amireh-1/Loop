#pragma warning disable IDE0037 // Member name can be simplified
using System;
using Loop.Domain.Common;
using Loop.Domain.Tiers;
using Loop.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Loop.Infrastructure.Users;

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

        builder.OwnsOne(u => u.Phone, pb =>
        {
            pb.Property(p => p.Value)
                .HasColumnName("phone")
                .IsRequired()
                .HasMaxLength(20);

            pb.HasIndex(p => p.Value).IsUnique();

           
        });

        builder.OwnsOne(u => u.Email, eb =>
        {
            eb.Property(e => e.Value)
                .HasColumnName("email")
                .IsRequired()
                .HasMaxLength(256);

            eb.HasIndex(e => e.Value).IsUnique();


        });

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

        builder.HasIndex(u => u.TierId);

        builder.HasOne(u => u.PointsBalance)
            .WithOne()
            .HasForeignKey<UserPointsBalance>(pb => pb.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        
    }
}

