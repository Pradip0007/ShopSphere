using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShopSphere.Domain.Catalog;
using ShopSphere.Infrastructure.Persistence.Converters;

namespace ShopSphere.Infrastructure.Persistence.Configurations;

internal sealed class ProductImageConfiguration : IEntityTypeConfiguration<ProductImage>
{
    public void Configure(EntityTypeBuilder<ProductImage> builder)
    {
        builder.ToTable("ProductImages");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Id)
            .ValueGeneratedNever();

        builder.Property(i => i.ProductId)
            .HasConversion(StronglyTypedIdConverters.ProductId)
            .IsRequired();

        builder.Property(i => i.StorageKey)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(i => i.ContentType)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(i => i.Width)
            .IsRequired();

        builder.Property(i => i.Height)
            .IsRequired();

        builder.Property(i => i.Position)
            .IsRequired();

        builder.HasIndex(i => new { i.ProductId, i.Position });

        builder.HasOne<Product>()
            .WithMany(p => p.Images)
            .HasForeignKey(i => i.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}