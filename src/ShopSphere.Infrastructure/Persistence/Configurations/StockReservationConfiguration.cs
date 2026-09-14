using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShopSphere.Domain.Inventory;

namespace ShopSphere.Infrastructure.Persistence.Configurations;

internal sealed class StockReservationConfiguration
    : IEntityTypeConfiguration<StockReservation>
{
    public void Configure(EntityTypeBuilder<StockReservation> builder)
    {
        builder.ToTable("StockReservations");

        builder.HasKey(x => new { x.OrderId, x.Sku });

        builder.Property(x => x.Sku)
            .HasMaxLength(64);

        builder.Property(x => x.Quantity)
            .IsRequired();

        builder.Property(x => x.ReservedAt)
            .IsRequired();
    }
}