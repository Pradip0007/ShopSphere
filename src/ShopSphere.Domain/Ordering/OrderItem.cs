using ShopSphere.Domain.Catalog;
using ShopSphere.Domain.Common;

namespace ShopSphere.Domain.Ordering;

public sealed class OrderItem : Entity<Guid>
{
    public ProductId ProductId { get; }
    public string Sku { get; }
    public string ProductNameSnapshot { get; }
    public Money UnitPriceSnapshot { get; }
    public int Quantity { get; }

    public Money LineTotal => UnitPriceSnapshot * Quantity;

    public OrderItem(
        ProductId productId,
        string sku,
        string productNameSnapshot,
        Money unitPriceSnapshot,
        int quantity) : base(Guid.NewGuid())
    {
        if (string.IsNullOrWhiteSpace(sku)) throw new ArgumentException("SKU required.", nameof(sku));
        if (string.IsNullOrWhiteSpace(productNameSnapshot)) throw new ArgumentException("Name required.", nameof(productNameSnapshot));
        if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be positive.");

        ProductId = productId;
        Sku = sku;
        ProductNameSnapshot = productNameSnapshot;
        UnitPriceSnapshot = unitPriceSnapshot;
        Quantity = quantity;
    }

    // EF materialisation
    private OrderItem() : base()
    {
        Sku = default!;
        ProductNameSnapshot = default!;
        UnitPriceSnapshot = default!;
    }
}
