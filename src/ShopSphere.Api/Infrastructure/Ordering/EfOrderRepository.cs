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

    public async Task<(IReadOnlyList<Order> Items, int TotalCount)> GetByUserAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        var query = db.Orders
            .AsNoTracking()
            .Include("_items")
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.PlacedAtUtc);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public Task SaveChangesAsync(CancellationToken ct = default) => db.SaveChangesAsync(ct);
}
