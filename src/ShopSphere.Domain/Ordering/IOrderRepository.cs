namespace ShopSphere.Domain.Ordering;

public interface IOrderRepository
{
    Task AddAsync(Order order, CancellationToken ct = default);
    Task<Order?> FindAsync(OrderId id, CancellationToken ct = default);
    Task<(IReadOnlyList<Order> Items, int TotalCount)> GetByUserAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}