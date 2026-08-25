using ShopSphere.Domain.Catalog;

namespace ShopSphere.Domain.Reviews;

public interface IReviewRepository
{
    Task<Review?> FindAsync(ReviewId id, CancellationToken ct = default);
    Task<bool> ExistsForUserAsync(Guid userId, ProductId productId, CancellationToken ct = default);
    IAsyncEnumerable<Review> ListApprovedForProductAsync(ProductId productId, CancellationToken ct = default);
    IAsyncEnumerable<Review> ListPendingAsync(CancellationToken ct = default);
    Task AddAsync(Review review, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}