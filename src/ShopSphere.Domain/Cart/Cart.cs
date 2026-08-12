namespace ShopSphere.Domain.Cart;

public sealed record Cart(CartKey Key, IReadOnlyList<CartLine> Lines)
{
    public bool IsEmpty => Lines.Count == 0;

    public int TotalUnits => Lines.Sum(l => l.Quantity);
}