using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ShopSphere.Domain.Catalog;

namespace ShopSphere.Infrastructure.Persistence.Converters;

internal static class CatalogValueObjectConverters
{
    public static ValueConverter<Slug, string> Slug { get; } =
        new(s => s.Value, v => ShopSphere.Domain.Catalog.Slug.From(v));

    public static ValueConverter<Sku, string> Sku { get; } =
        new(s => s.Value, v => ShopSphere.Domain.Catalog.Sku.From(v));
}