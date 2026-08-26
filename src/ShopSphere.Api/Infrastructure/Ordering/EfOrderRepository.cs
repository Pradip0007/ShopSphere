using Microsoft.EntityFrameworkCore;
using ShopSphere.Infrastructure.Persistence;
using ShopSphere.Domain.Ordering;

namespace ShopSphere.Api.Infrastructure.Ordering;

public sealed class EfOrderRepository(ShopSphereDbContext db) : IOrderRepository
{
    public async Task AddAsync(Order order, CancellationToken ct = default)
    {
        await db.Orders.AddAsync(order, ct);
    }

    public Task<Order?> FindAsync(OrderId id, CancellationToken ct = default)
    {
        return db.Orders
            .Include("_items")
            .FirstOrDefaultAsync(o => o.Id == id, ct);
    }

    public Task SaveChangesAsync(CancellationToken ct = default) => db.SaveChangesAsync(ct);
}
