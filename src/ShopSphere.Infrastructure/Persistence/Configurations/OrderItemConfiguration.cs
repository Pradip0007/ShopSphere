using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShopSphere.Domain.Catalog;
using ShopSphere.Domain.Ordering;

namespace ShopSphere.Infrastructure.Persistence.Configurations;

public sealed class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> b)
    {
        b.ToTable("OrderItems");
        b.HasKey(i => i.Id);

        b.Property(i => i.ProductId)
            .HasConversion(id => id.Value, v => new ProductId(v))
            .IsRequired();

        b.Property(i => i.Sku).HasMaxLength(64).IsRequired();
        b.Property(i => i.ProductNameSnapshot).HasMaxLength(200).IsRequired();
        b.Property(i => i.Quantity).IsRequired();

        // Ignore the derived property — it's computed at read time.
        b.Ignore(i => i.LineTotal);

        b.OwnsOne(i => i.UnitPriceSnapshot, m =>
        {
            m.Property(x => x.Amount).HasColumnName("UnitPriceAmount").HasPrecision(18, 4);
            m.Property(x => x.Currency).HasColumnName("UnitPriceCurrency").HasMaxLength(3);
        });

        b.HasIndex("OrderId");
    }
}