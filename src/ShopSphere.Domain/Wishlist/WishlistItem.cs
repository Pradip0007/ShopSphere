using ShopSphere.Domain.Catalog;

namespace ShopSphere.Domain.Wishlist;

public sealed record WishlistItem(
    ProductId ProductId,
    DateTimeOffset AddedAtUtc);