using Microsoft.EntityFrameworkCore;
using ShopSphere.Domain.Ordering;

namespace ShopSphere.Infrastructure.Persistence;

public sealed class SqlOrderRepository(ShopSphereDbContext db) : IOrderRepository
{
    public async Task AddAsync(Order order, CancellationToken ct = default)
    {
        await db.Orders.AddAsync(order, ct);
    }

    public Task<Order?> FindAsync(OrderId id, CancellationToken ct = default)
    {
        return db.Orders
            .Include("_items")           // load private items collection
            .FirstOrDefaultAsync(o => o.Id == id, ct);
    }

    public Task SaveChangesAsync(CancellationToken ct = default)
        => db.SaveChangesAsync(ct);
}