using ShopSphere.Domain.Catalog;
using ShopSphere.Domain.Common;
using ShopSphere.Domain.Users;
using ShopSphere.Domain.Wishlist.Events;

namespace ShopSphere.Domain.Wishlist;

public sealed class Wishlist : AggregateRoot<WishlistId>
{
    public UserId UserId { get; private set; }

    private readonly List<WishlistItem> _items = [];

    public IReadOnlyList<WishlistItem> Items => _items;

    public static Wishlist For(UserId user) =>
        new()
        {
            Id = WishlistId.New(),
            UserId = user,
        };

    public Result Add(ProductId product, TimeProvider clock)
    {
        if (_items.Any(i => i.ProductId == product))
            return Result.Success();

        var now = clock.GetUtcNow();

        _items.Add(new WishlistItem(product, now));

        Raise(new WishlistItemAddedEvent(
            Id,
            UserId,
            product));

        return Result.Success();
    }
    public Result Remove(ProductId product)
    {
        var item = _items.FirstOrDefault(i => i.ProductId == product);

        if (item is null)
            return Result.Success();

        _items.Remove(item);

        return Result.Success();
    }
}