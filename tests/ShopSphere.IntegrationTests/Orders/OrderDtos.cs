namespace ShopSphere.IntegrationTests.Orders;

public sealed record CheckoutRequest(
    ShippingAddressRequest ShippingAddress);

public sealed record ShippingAddressRequest(
    string Line1,
    string? Line2,
    string City,
    string PostalCode,
    string Country);

public sealed record CheckoutResponse(
    Guid OrderId,
    decimal Total,
    string Currency,
    int LineCount);

public sealed record OrderResponse(
    Guid Id,
    string Status,
    decimal TotalAmount,
    string Currency);
