using ShopSphere.Domain.Catalog.Events;
using ShopSphere.Domain.Common;

namespace ShopSphere.Domain.Catalog;

/// <summary>
/// A sellable item. State transitions: Draft → Published → Archived.
/// Only Draft can be Published. Only Published can be Archived.
/// Search: Title + Description are matched by SearchProductsHandler using
/// SQL LIKE. Day 85 introduces a ProductEmbedding sibling entity and a
/// hybrid search combining this table with vector similarity.
/// </summary>
public sealed class Product : AggregateRoot<ProductId>
{
    // EF ctor
    private Product() { }

    private Product(
        ProductId id,
        string title,
        string description,
        Slug slug,
        Sku sku,
        CategoryId categoryId,
        Money price) : base(id)
    {
        Title = title;
        Description = description;
        Slug = slug;
        Sku = sku;
        CategoryId = categoryId;
        Price = price;
        Status = ProductStatus.Draft;
    }

    public string Title { get; private set; } = default!;
    public string Description { get; private set; } = default!;
    public Slug Slug { get; private set; } = default!;
    public Sku Sku { get; private set; } = default!;
    public CategoryId CategoryId { get; private set; }
    public Money Price { get; private set; } = default!;
    public ProductStatus Status { get; private set; }

    /// <summary>
    /// Factory. New products start in Draft — no publish event yet.
    /// </summary>
    public static Product Create(
        string title,
        string description,
        Sku sku,
        CategoryId categoryId,
        Money price,
        Slug? slug = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        if (title.Length > 200)
            throw new ArgumentException("Title must be 200 characters or fewer.", nameof(title));
        ArgumentNullException.ThrowIfNull(description);
        ArgumentNullException.ThrowIfNull(sku);
        ArgumentNullException.ThrowIfNull(price);
        if (price.Amount < 0)
            throw new ArgumentException("Price cannot be negative.", nameof(price));

        var resolvedSlug = slug ?? Slug.FromName(title);
        var id = ProductId.New();

        return new Product(id, title.Trim(), description, resolvedSlug, sku, categoryId, price);
    }

    public void Rename(string title, Slug? slug = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);

        if (title.Length > 200)
            throw new ArgumentException(
                "Title must be 200 characters or fewer.",
                nameof(title));

        Title = title.Trim();
        Slug = slug ?? Slug.FromName(title);

        // TODO: Raise ProductRenamedEvent
    }

    public void ChangePrice(Money newPrice)
    {
        ArgumentNullException.ThrowIfNull(newPrice);

        if (newPrice.Amount < 0)
            throw new ArgumentException(
                "Price cannot be negative.",
                nameof(newPrice));

        Price = newPrice;

        // TODO: Raise ProductPriceChangedEvent
    }

    public void UpdateDescription(string description)
    {
        ArgumentNullException.ThrowIfNull(description);
        if (description.Length > 4000)
        {
            throw new ArgumentException("Description exceeds 4000 characters.", nameof(description));
        }
        Description = description;
    }

    public void Publish()
    {
        if (Status != ProductStatus.Draft)
            throw new InvalidOperationException($"Only Draft products can be published (was {Status}).");

        Status = ProductStatus.Published;
        Raise(new ProductPublishedEvent(Id, Sku, CategoryId, Price));
    }

    public void Archive()
    {
        if (Status != ProductStatus.Published)
            throw new InvalidOperationException($"Only Published products can be archived (was {Status}).");

        Status = ProductStatus.Archived;
    }

    public void Unarchive()
    {
        if (Status != ProductStatus.Archived)
            throw new InvalidOperationException(
                $"Only Archived products can be unarchived (was {Status}).");

        Status = ProductStatus.Draft;

        // TODO: Raise ProductUnarchivedEvent
    }
}