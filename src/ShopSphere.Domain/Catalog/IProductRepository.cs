namespace ShopSphere.Domain.Catalog;

public interface IProductRepository
{
    Task<Product?> FindAsync(
        ProductId id,
        CancellationToken ct = default);
}