namespace ShopSphere.Domain.Inventory;

public readonly record struct StockLevelId(Guid Value)
{
    public static StockLevelId New() => new(Guid.NewGuid());
    public override string ToString() => Value.ToString();
}
