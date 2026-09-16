namespace ShopSphere.IntegrationTests.Catalog;

internal sealed record CreateProductRequest(
    string Title,
    string Sku,
    string Description,
    decimal Price,
    string Currency,
    int InitialStock,
    Guid CategoryId);

internal sealed record CreateProductResponse(Guid Id);

internal sealed record ProductListItemResponse(
    Guid Id,
    string Title,
    string Slug,
    string Sku,
    decimal Price,
    string Currency,
    Guid CategoryId);

internal sealed record ProductListResponse(
    ProductListItemResponse[] Items,
    int Page,
    int PageSize,
    int TotalCount);

internal sealed record ProductDetailResponse(
    Guid Id,
    string Title,
    string Slug,
    string Sku,
    decimal Price,
    string Currency,
    Guid CategoryId,
    string Category,
    IReadOnlyList<string> Images,
    string LongDescription,
    int Stock,
    IReadOnlyList<ProductAttributeResponse> Attributes,
    string ShippingInfo,
    double? AverageRating,
    int RatingCount);

internal sealed record ProductAttributeResponse(string Name, string Value);

internal sealed record UpdateProductRequest(
    string? Title,
    string? Description,
    decimal? Price,
    string? Currency);
