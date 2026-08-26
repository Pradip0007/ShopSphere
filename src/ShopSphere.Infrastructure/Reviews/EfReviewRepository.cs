using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using ShopSphere.Domain.Catalog;
using ShopSphere.Domain.Reviews;
using ShopSphere.Infrastructure.Persistence;

namespace ShopSphere.Infrastructure.Reviews;

public sealed class EfReviewRepository(ShopSphereDbContext db) : IReviewRepository
{
    public Task<Review?> FindAsync(ReviewId id, CancellationToken ct = default) =>
        db.Reviews.FirstOrDefaultAsync(r => r.Id == id, ct);

    public Task<bool> ExistsForUserAsync(Guid userId, ProductId productId, CancellationToken ct = default) =>
        db.Reviews.AnyAsync(r => r.UserId == userId && r.ProductId == productId, ct);

    public async IAsyncEnumerable<Review> ListApprovedForProductAsync(
    ProductId productId,
    [EnumeratorCancellation] CancellationToken ct = default)
    {
        var reviews = await db.Reviews
            .AsNoTracking()
            .Where(r => r.ProductId == productId &&
                        r.Status == ReviewStatus.Approved)
            .ToListAsync(ct);

        foreach (var review in reviews.OrderByDescending(r => r.PostedAtUtc))
        {
            yield return review;
        }
    }

    public async IAsyncEnumerable<Review> ListPendingAsync(
    [EnumeratorCancellation] CancellationToken ct = default)
    {
        var reviews = await db.Reviews
            .Where(r => r.Status == ReviewStatus.Pending)
            .ToListAsync(ct);

        foreach (var review in reviews.OrderBy(r => r.PostedAtUtc))
        {
            yield return review;
        }
    }
    public async Task AddAsync(Review review, CancellationToken ct = default) =>
        await db.Reviews.AddAsync(review, ct);

    public Task SaveChangesAsync(CancellationToken ct = default) => db.SaveChangesAsync(ct);
}