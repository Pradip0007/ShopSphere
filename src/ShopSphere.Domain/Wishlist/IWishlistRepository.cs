using ShopSphere.Domain.Catalog;
using ShopSphere.Domain.Users;

namespace ShopSphere.Domain.Wishlist;

public interface IWishlistRepository
{
    Task<Wishlist?> GetAsync(UserId userId, CancellationToken ct = default);

    Task AddItemAsync(
        Wishlist wishlist,
        ProductId productId,
        TimeProvider clock,
        CancellationToken ct = default);

    Task RemoveItemAsync(
        Wishlist wishlist,
        ProductId productId,
        CancellationToken ct = default);

    Task SaveAsync(
        Wishlist wishlist,
        CancellationToken ct = default);
}