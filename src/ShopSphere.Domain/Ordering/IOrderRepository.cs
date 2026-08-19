namespace ShopSphere.Domain.Ordering;

public interface IOrderRepository
{
    Task AddAsync(Order order, CancellationToken ct = default);
    Task<Order?> FindAsync(OrderId id, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}