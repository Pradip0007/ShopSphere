using ShopSphere.Domain.Catalog.Events;
using ShopSphere.Domain.Common;

namespace ShopSphere.Domain.Catalog;

/// <summary>
/// Self-referencing tree node. Every category has a name and slug;
/// a nullable ParentId lets categories nest arbitrarily deep.
/// </summary>
public sealed class Category : AggregateRoot<CategoryId>
{
    // EF ctor
    private Category() { }

    private Category(CategoryId id, string name, Slug slug, CategoryId? parentId) : base(id)
    {
        Name = name;
        Slug = slug;
        ParentId = parentId;
    }

    public string Name { get; private set; } = default!;
    public Slug Slug { get; private set; } = default!;
    public CategoryId? ParentId { get; private set; }

    /// <summary>
    /// Factory. Derives slug from name unless one is supplied explicitly.
    /// </summary>
    public static Category Create(string name, Slug? slug = null, CategoryId? parentId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        if (name.Length > 200)
            throw new ArgumentException("Category name must be 200 characters or fewer.", nameof(name));

        var resolvedSlug = slug ?? Slug.FromName(name);
        var id = CategoryId.New();

        var category = new Category(id, name.Trim(), resolvedSlug, parentId);
        category.Raise(new CategoryCreatedEvent(id, category.Name, resolvedSlug, parentId));
        return category;
    }

    public void Rename(string newName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(newName);
        Name = newName.Trim();
    }

    public void MoveTo(CategoryId? newParentId)
    {
        if (newParentId is { } id && id == Id)
            throw new InvalidOperationException("A category cannot be its own parent.");
        ParentId = newParentId;
    }
}
