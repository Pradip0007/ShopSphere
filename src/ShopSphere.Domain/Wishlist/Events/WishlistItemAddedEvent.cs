using ShopSphere.Domain.Catalog;
using ShopSphere.Domain.Common;
using ShopSphere.Domain.Users;

namespace ShopSphere.Domain.Wishlist.Events;

public sealed record WishlistItemAddedEvent(
    WishlistId WishlistId,
    UserId UserId,
    ProductId ProductId) : DomainEvent;