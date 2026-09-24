using ShopSphere.Domain.Common;

namespace ShopSphere.Domain.Catalog;

public sealed class ProductImage : Entity<Guid>
{
    private ProductImage()
    {
    }

    private ProductImage(Guid id)
        : base(id)
    {
    }

    public ProductId ProductId { get; private set; }

    public string StorageKey { get; private set; } = string.Empty;

    public string ContentType { get; private set; } = string.Empty;

    public int Width { get; private set; }

    public int Height { get; private set; }

    public int Position { get; private set; }

    public static ProductImage For(
        ProductId productId,
        string storageKey,
        string contentType,
        int width,
        int height,
        int position)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(storageKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(contentType);

        if (width < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(width));
        }

        if (height < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(height));
        }

        if (position < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(position));
        }

        return new ProductImage(Guid.NewGuid())
        {
            ProductId = productId,
            StorageKey = storageKey,
            ContentType = contentType,
            Width = width,
            Height = height,
            Position = position
        };
    }
}