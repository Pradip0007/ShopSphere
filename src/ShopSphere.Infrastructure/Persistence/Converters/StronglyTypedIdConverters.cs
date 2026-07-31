using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ShopSphere.Domain.Catalog;
using ShopSphere.Domain.Inventory;

namespace ShopSphere.Infrastructure.Persistence.Converters;

internal static class StronglyTypedIdConverters
{
    public static ValueConverter<ProductId, Guid> ProductId { get; } =
        new(id => id.Value, value => new ProductId(value));

    public static ValueConverter<CategoryId, Guid> CategoryId { get; } =
        new(id => id.Value, value => new CategoryId(value));

    public static ValueConverter<CategoryId?, Guid?> NullableCategoryId { get; } =
        new(
            id => id.HasValue ? id.Value.Value : (Guid?)null,
            value => value.HasValue ? new CategoryId(value.Value) : null);

    public static ValueConverter<StockLevelId, Guid> StockLevelId { get; } =
        new(id => id.Value, value => new StockLevelId(value));
}
