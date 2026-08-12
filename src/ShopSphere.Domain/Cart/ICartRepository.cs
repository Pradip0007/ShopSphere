using ShopSphere.Domain.Catalog;

namespace ShopSphere.Domain.Cart;

public interface ICartRepository
{
    Task<Cart> GetAsync(CartKey key, CancellationToken ct = default);

    Task AddItemAsync(CartKey key, ProductId productId, int quantity, CancellationToken ct = default);

    Task UpdateItemAsync(CartKey key, ProductId productId, int quantity, CancellationToken ct = default);

    Task RemoveItemAsync(CartKey key, ProductId productId, CancellationToken ct = default);

    Task ClearAsync(CartKey key, CancellationToken ct = default);
}