using ShopSphere.Domain.Catalog;
using ShopSphere.Domain.Common;
using ShopSphere.Domain.Inventory.Events;

namespace ShopSphere.Domain.Inventory;

/// <summary>
/// Tracks Available and Reserved counts for a single product.
/// Invariants: Available >= 0, Reserved >= 0.
/// Reserve moves units Available → Reserved; Release moves them back.
/// Adjust changes Available directly (stock take, replenishment).
/// </summary>
public sealed class StockLevel : AggregateRoot<StockLevelId>
{
    // EF ctor
    private StockLevel() { }

    private StockLevel(StockLevelId id, ProductId productId, int available) : base(id)
    {
        ProductId = productId;
        Available = available;
        Reserved = 0;
    }

    public ProductId ProductId { get; private set; }
    public int Available { get; private set; }
    public int Reserved { get; private set; }

    public static StockLevel Create(ProductId productId, int initialAvailable = 0)
    {
        if (initialAvailable < 0)
            throw new ArgumentOutOfRangeException(nameof(initialAvailable), "Initial stock cannot be negative.");
        return new StockLevel(StockLevelId.New(), productId, initialAvailable);
    }

    /// <summary>
    /// Move <paramref name="quantity"/> units from Available to Reserved.
    /// Fails (does not throw) when the request exceeds Available.
    /// </summary>
    public Result Reserve(int quantity)
    {
        if (quantity <= 0)
        return Result.Failure(InventoryErrors.QuantityNotPositive("Reserve"));
        if (quantity > Available)
            return Result.Failure(InventoryErrors.InsufficientStock(quantity, Available));

        Available -= quantity;
        Reserved += quantity;

        Raise(new StockReservedEvent(Id, ProductId, quantity, Available, Reserved));

        if (Available == 0)
            Raise(new StockDepletedEvent(Id, ProductId));

        return Result.Success();
    }

    /// <summary>
    /// Move <paramref name="quantity"/> units from Reserved back to Available
    /// (e.g. cart abandoned, order cancelled).
    /// </summary>
    public Result Release(int quantity)
    {
        if (quantity <= 0)
        return Result.Failure(InventoryErrors.QuantityNotPositive("Release"));
        if (quantity > Reserved)
            return Result.Failure(InventoryErrors.InsufficientReserved(quantity, Reserved));

        Reserved -= quantity;
        Available += quantity;

        Raise(new StockReleasedEvent(Id, ProductId, quantity, Available, Reserved));
        return Result.Success();
    }

    /// <summary>
    /// Absolute adjustment to Available. Positive delta = replenishment,
    /// negative = shrinkage/stock-take. Reserved is unaffected.
    /// </summary>
    public Result Adjust(int delta)
    {
        var newAvailable = Available + delta;
        if (newAvailable < 0)
            return Result.Failure(InventoryErrors.AdjustmentBelowZero(Available, delta));

        var wasNonZero = Available > 0;
        Available = newAvailable;

        if (wasNonZero && Available == 0)
            Raise(new StockDepletedEvent(Id, ProductId));

        return Result.Success();
    }
}