using Domain.Common;
using Domain.Malls;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Malls;

internal sealed class MallAdminConfiguration : IEntityTypeConfiguration<MallAdmin>
{
    public void Configure(EntityTypeBuilder<MallAdmin> builder)
    {
        builder.HasKey(ma => ma.MallAdminId);

        builder.Property(ma => ma.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(sa => sa.Email)
            .IsRequired()
            .HasMaxLength(256)
            .HasConversion(e => e.Value, v => Email.Create(v));

        builder.Property(sa => sa.Phone)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion(p => p.Value, v => Phone.Create(v));

        builder.Property(ma => ma.PasswordHash)
            .IsRequired();

        builder.Property(ma => ma.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("now()");
       
        builder.Property(ma => ma.MallId)
    .IsRequired();

        builder.HasOne<Mall>()
            .WithMany()
            .HasForeignKey(x=>x.MallId);

        builder.Property(sa => sa.Email)
.IsRequired()
.HasMaxLength(256)
.HasConversion(e => e.Value, v => Email.Create(v));

        builder.Property(sa => sa.Phone)
           .IsRequired()
           .HasMaxLength(20)
           .HasConversion(p => p.Value, v => Phone.Create(v));

        builder.HasIndex(ma => ma.Email).IsUnique();
        builder.HasIndex(ma => ma.Phone).IsUnique();
    }
}
