using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShopSphere.Domain.Catalog;
using ShopSphere.Infrastructure.Persistence.Converters;

namespace ShopSphere.Infrastructure.Persistence.Configurations;

internal sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id)
               .HasConversion(StronglyTypedIdConverters.ProductId)
               .ValueGeneratedNever();

        builder.Property(p => p.Title)
               .HasMaxLength(200)
               .IsRequired();

        builder.Property(p => p.Description)
               .HasMaxLength(4000)
               .IsRequired();

        builder.Property(p => p.Slug)
               .HasConversion(CatalogValueObjectConverters.Slug)
               .HasMaxLength(220)
               .IsRequired();
        builder.HasIndex(p => p.Slug).IsUnique();

        builder.Property(p => p.Sku)
               .HasConversion(CatalogValueObjectConverters.Sku)
               .HasMaxLength(32)
               .IsRequired();
        builder.HasIndex(p => p.Sku).IsUnique();

        builder.Property(p => p.CategoryId)
               .HasConversion(StronglyTypedIdConverters.CategoryId)
               .IsRequired();

        builder.HasOne<Category>()
               .WithMany()
               .HasForeignKey(p => p.CategoryId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.OwnsOne(p => p.Price, price =>
        {
            price.Property(m => m.Amount)
                 .HasColumnName("PriceAmount")
                 .HasColumnType("decimal(18,4)")
                 .IsRequired();
            price.Property(m => m.Currency)
                 .HasColumnName("PriceCurrency")
                 .HasMaxLength(3)
                 .IsFixedLength()
                 .IsRequired();
        });

        builder.Property(p => p.Status)
               .HasConversion<int>()
               .IsRequired();

        builder.HasIndex(p => p.Status); // used by "list published" queries

        builder.Ignore(p => p.DomainEvents);
    }
}