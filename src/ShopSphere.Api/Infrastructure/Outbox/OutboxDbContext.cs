using Microsoft.EntityFrameworkCore;
using ShopSphere.Domain.Catalog;
using ShopSphere.Domain.Ordering;

namespace ShopSphere.Api.Infrastructure.Outbox;

public sealed class OutboxDbContext(DbContextOptions<OutboxDbContext> options) : DbContext(options)
{
    public DbSet<OutboxMessage> Outbox => Set<OutboxMessage>();
    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        base.OnModelCreating(modelBuilder);

        var e = modelBuilder.Entity<OutboxMessage>();
        e.ToTable("Outbox");
        e.HasKey(x => x.Id);
        e.Property(x => x.Type).HasMaxLength(500).IsRequired();
        e.Property(x => x.PayloadJson).IsRequired();
        e.HasIndex(x => x.ProcessedAtUtc)
            .HasFilter("\"ProcessedAtUtc\" IS NULL")
            .HasDatabaseName("IX_Outbox_Pending");

        modelBuilder.Entity<Order>(o =>
        {
            o.ToTable("Orders");
            o.HasKey(x => x.Id);
            o.Property(x => x.Id)
                .HasConversion(id => id.Value, v => new OrderId(v));
            o.Property(x => x.UserId).IsRequired();
            o.Property(x => x.Status).HasConversion<int>();
            o.Property(x => x.Currency).HasMaxLength(3).IsRequired();
            o.Property(x => x.PlacedAtUtc).IsRequired();

            o.OwnsOne(x => x.Subtotal, m =>
            {
                m.Property(p => p.Amount).HasColumnName("SubtotalAmount").HasPrecision(18, 4);
                m.Property(p => p.Currency).HasColumnName("SubtotalCurrency").HasMaxLength(3);
            });

            o.OwnsOne(x => x.ShippingAddress, a =>
            {
                a.Property(x => x.Line1).HasColumnName("ShipLine1").HasMaxLength(200);
                a.Property(x => x.Line2).HasColumnName("ShipLine2").HasMaxLength(200);
                a.Property(x => x.City).HasColumnName("ShipCity").HasMaxLength(120);
                a.Property(x => x.PostalCode).HasColumnName("ShipPostalCode").HasMaxLength(20);
                a.Property(x => x.Country).HasColumnName("ShipCountry").HasMaxLength(2);
            });

            o.HasMany<OrderItem>("_items")
                .WithOne()
                .HasForeignKey("OrderId")
                .OnDelete(DeleteBehavior.Cascade);

            o.Metadata.FindNavigation("_items")!
                .SetPropertyAccessMode(PropertyAccessMode.Field);

            o.Ignore(x => x.Items);
            o.Ignore(x => x.DomainEvents);
        });

        modelBuilder.Entity<OrderItem>(i =>
        {
            i.ToTable("OrderItems");
            i.HasKey("Id");
            i.Property<Guid>("Id");
            i.Property(x => x.ProductId)
                .HasConversion(id => id.Value, v => new ProductId(v));
            i.Property(x => x.Sku).HasMaxLength(128).IsRequired();
            i.Property(x => x.ProductNameSnapshot).HasMaxLength(200).IsRequired();
            i.OwnsOne(x => x.UnitPriceSnapshot, m =>
            {
                m.Property(p => p.Amount).HasColumnName("UnitPriceAmount").HasPrecision(18, 4);
                m.Property(p => p.Currency).HasColumnName("UnitPriceCurrency").HasMaxLength(3);
            });
            i.Property(x => x.Quantity).IsRequired();
        });
    }
}
