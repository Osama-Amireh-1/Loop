using Domain.Stamps;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Stamps;

internal sealed class UserStampCardConfiguration : IEntityTypeConfiguration<UserStampCard>
{
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

        builder.HasOne<Stamp>(usc=> usc.Stamp)
            .WithMany()
            .HasForeignKey(usc => usc.StampId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(usc => usc.UserId);
        builder.HasIndex(usc => usc.StampId);

    }
}
