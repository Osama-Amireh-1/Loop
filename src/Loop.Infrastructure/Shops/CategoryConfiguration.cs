using Domain.Shops;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Shops;

internal sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.HasKey(c => c.CategoryId);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.IconUrl)
            .HasMaxLength(512);

        builder.Property(c => c.DisplayOrder)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(c => c.Description)
            .HasColumnType("text");

        builder.Property(c => c.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("now()");

        builder.HasIndex(c => c.Name).IsUnique();
    }
}
