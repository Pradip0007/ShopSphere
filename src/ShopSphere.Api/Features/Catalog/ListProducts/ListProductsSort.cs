namespace ShopSphere.Api.Features.Catalog.ListProducts;

public static class ListProductsSort
{
    public const string PriceAsc = "price_asc";
    public const string PriceDesc = "price_desc";
    public const string Name = "name";

    public static readonly IReadOnlySet<string> Allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        PriceAsc,
        PriceDesc,
        Name,
    };
}
