using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShopSphere.Domain.Ordering;
using ShopSphere.Infrastructure.Persistence.Converters;

namespace ShopSphere.Infrastructure.Persistence.Configurations;

public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> b)
    {
        b.ToTable("Orders");
        b.HasKey(o => o.Id);

        b.Property(o => o.Id)
            .HasConversion(id => id.Value, v => new OrderId(v));

        b.Property(o => o.UserId).IsRequired();

        b.Property(o => o.Status)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        b.Property(o => o.Currency).HasMaxLength(3).IsRequired();
        b.Property(o => o.PlacedAtUtc).IsRequired();

        // Owned Money — shares column names with Product for consistency.
        b.OwnsOne(o => o.Subtotal, m =>
        {
            m.Property(x => x.Amount).HasColumnName("SubtotalAmount").HasPrecision(18, 4);
            m.Property(x => x.Currency).HasColumnName("SubtotalCurrency").HasMaxLength(3);
        });

        // Owned Address.
        b.OwnsOne(o => o.ShippingAddress, a =>
        {
            a.Property(x => x.Line1).HasColumnName("ShipLine1").HasMaxLength(200);
            a.Property(x => x.Line2).HasColumnName("ShipLine2").HasMaxLength(200);
            a.Property(x => x.City).HasColumnName("ShipCity").HasMaxLength(120);
            a.Property(x => x.PostalCode).HasColumnName("ShipPostalCode").HasMaxLength(20);
            a.Property(x => x.Country).HasColumnName("ShipCountry").HasMaxLength(2);
        });

        // Items collection — private backing field.
        b.HasMany<OrderItem>("_items")
            .WithOne()
            .HasForeignKey("OrderId")
            .OnDelete(DeleteBehavior.Cascade);

        b.Metadata.FindNavigation("_items")!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        b.Ignore(o => o.Items);           // expose the read-only view, don't map it
        b.Ignore(o => o.DomainEvents);    // domain events aren't persisted
    }
}