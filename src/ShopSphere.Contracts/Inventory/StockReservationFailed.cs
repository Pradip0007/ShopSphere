namespace ShopSphere.Contracts.Inventory;

public sealed record StockReservationFailed(
    Guid OrderId,
    string Sku,
    int Quantity,
    string Reason,
    string Detail);