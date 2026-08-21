using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShopSphere.Domain.Inventory;
using ShopSphere.Infrastructure.Persistence.Converters;
using ShopSphere.Domain.Catalog;

namespace ShopSphere.Infrastructure.Persistence.Configurations;

internal sealed class StockLevelConfiguration : IEntityTypeConfiguration<StockLevel>
{
    public void Configure(EntityTypeBuilder<StockLevel> builder)
    {
        builder.ToTable("StockLevels");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id)
               .HasConversion(StronglyTypedIdConverters.StockLevelId)
               .ValueGeneratedNever();

        builder.Property(s => s.ProductId)
               .HasConversion(StronglyTypedIdConverters.ProductId)
               .IsRequired();

        builder.Property(s => s.Sku)
              .HasConversion(sku => sku.Value, v => Sku.From(v))
              .HasColumnName("Sku")
              .HasMaxLength(64)
              .IsRequired();
       
        builder.HasIndex(s => s.Sku).IsUnique();

        builder.HasIndex(s => s.ProductId).IsUnique(); // one stock row per product

        builder.Property(s => s.Available).IsRequired();
        builder.Property(s => s.Reserved).IsRequired();

        // Optimistic concurrency — SQL Server rowversion.
        builder.Property<byte[]>("RowVersion")
               .IsRowVersion()
               .IsConcurrencyToken();

        builder.Ignore(s => s.DomainEvents);
    }
}