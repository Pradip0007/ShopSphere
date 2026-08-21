using System.Collections.Concurrent;
using ShopSphere.Domain.Ordering;

namespace ShopSphere.Api.Infrastructure.Ordering;

public sealed class InMemoryOrderRepository : IOrderRepository
{
    private readonly ConcurrentDictionary<OrderId, Order> _store = new();

    public Task AddAsync(Order order, CancellationToken ct = default)
    {
        _store[order.Id] = order;
        return Task.CompletedTask;
    }

    public Task<Order?> FindAsync(OrderId id, CancellationToken ct = default)
    {
        _store.TryGetValue(id, out var o);
        return Task.FromResult(o);
    }

    public Task SaveChangesAsync(CancellationToken ct = default) => Task.CompletedTask;
}