using Microsoft.EntityFrameworkCore;
using ShopSphere.Domain.Catalog;

namespace ShopSphere.Infrastructure.Persistence;

public sealed class ProductRepository(
    ShopSphereDbContext db) : IProductRepository
{
    public Task<Product?> FindAsync(
        ProductId id,
        CancellationToken ct = default)
    {
        return db.Products
            .FirstOrDefaultAsync(p => p.Id == id, ct);
    }
}