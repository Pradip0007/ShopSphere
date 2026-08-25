using Microsoft.EntityFrameworkCore;
using ShopSphere.Domain.Catalog;
using ShopSphere.Domain.Ordering;
using ShopSphere.Domain.Reviews;
using ShopSphere.Infrastructure.Audit;

namespace ShopSphere.Infrastructure.Outbox;

public sealed class OutboxDbContext(DbContextOptions<OutboxDbContext> options) : DbContext(options)
{
    public DbSet<OutboxMessage> Outbox => Set<OutboxMessage>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

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

        modelBuilder.Entity<Review>(r =>
        {
            r.ToTable("Reviews");

            r.HasKey(x => x.Id);

            r.Property(x => x.Id)
                .HasConversion(
                    id => id.Value,
                    value => new ReviewId(value));

            r.Property(x => x.UserId)
                .IsRequired();

            r.Property(x => x.ProductId)
                .HasConversion(
                    id => id.Value,
                    value => new ProductId(value))
                .IsRequired();

            r.Property(x => x.Rating)
                .IsRequired();

            r.Property(x => x.Body)
                .HasMaxLength(4000)
                .IsRequired();

            r.Property(x => x.Status)
                .HasConversion<int>()
                .IsRequired();

            r.Property(x => x.PostedAtUtc)
                .IsRequired();

            r.Property(x => x.ModeratedAtUtc);

            r.Property(x => x.ModeratorUserId);

            r.Property(x => x.RejectionReason)
                .HasMaxLength(1000);

            // One review per user per product.
            r.HasIndex(x => new { x.UserId, x.ProductId })
                .IsUnique();

            // Useful for public product review queries.
            r.HasIndex(x => new { x.ProductId, x.Status });
        });

        modelBuilder.Entity<AuditLog>(a =>
        {
            a.ToTable("AuditLogs");

            a.HasKey(x => x.Id);

            a.Property(x => x.EntityType)
                .HasMaxLength(200)
                .IsRequired();

            a.Property(x => x.EntityId)
                .HasMaxLength(64)
                .IsRequired();

            a.Property(x => x.Action)
                .HasConversion<int>();

            a.Property(x => x.PayloadJson)
                .IsRequired();

            a.Property(x => x.IpAddress)
                .HasMaxLength(64);

            a.Property(x => x.UserAgent)
                .HasMaxLength(500);

            a.HasIndex(x => new { x.EntityType, x.EntityId });

            a.HasIndex(x => x.TimestampUtc);
        });
    }
}
