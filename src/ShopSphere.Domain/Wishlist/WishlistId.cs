namespace ShopSphere.Domain.Wishlist;

public readonly record struct WishlistId(Guid Value)
{
    public static WishlistId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}