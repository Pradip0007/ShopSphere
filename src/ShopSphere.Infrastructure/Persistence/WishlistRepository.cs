using Microsoft.EntityFrameworkCore;
using ShopSphere.Domain.Catalog;
using ShopSphere.Domain.Users;
using ShopSphere.Domain.Wishlist;

namespace ShopSphere.Infrastructure.Persistence;

public sealed class WishlistRepository(
    ShopSphereDbContext db) : IWishlistRepository
{
    public async Task<Wishlist?> GetAsync(
        UserId userId,
        CancellationToken ct = default)
    {
        return await db.Wishlists
            .Include(w => w.Items)
            .FirstOrDefaultAsync(w => w.UserId == userId, ct);
    }

    public async Task AddItemAsync(
    Wishlist wishlist,
    ProductId productId,
    TimeProvider clock,
    CancellationToken ct = default)
    {
        if (db.Entry(wishlist).State == EntityState.Detached)
        {
            db.Wishlists.Add(wishlist);
        }

        wishlist.Add(productId, clock);

        await db.SaveChangesAsync(ct);
    }

    public async Task RemoveItemAsync(
        Wishlist wishlist,
        ProductId productId,
        CancellationToken ct = default)
    {
        wishlist.Remove(productId);
        await db.SaveChangesAsync(ct);
    }

    public async Task SaveAsync(
    Wishlist wishlist,
    CancellationToken ct = default)
    {
        if (db.Entry(wishlist).State == EntityState.Detached)
        {
            db.Wishlists.Add(wishlist);
        }

        await db.SaveChangesAsync(ct);
    }
}