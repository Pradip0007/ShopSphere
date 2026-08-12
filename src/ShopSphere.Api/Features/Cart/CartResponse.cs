namespace ShopSphere.Api.Features.Cart;

public sealed record CartLineResponse(string ProductId, int Quantity);

public sealed record CartResponse(
    string Key,
    int TotalUnits,
    IReadOnlyList<CartLineResponse> Lines);