using ShopSphere.Domain.Catalog;

namespace ShopSphere.Domain.Cart;

public sealed record CartLine(ProductId ProductId, int Quantity);