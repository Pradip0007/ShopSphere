namespace ShopSphere.Domain.Ordering;

public enum OrderStatus
{
    Pending = 0,
    InventoryReserved = 10,
    PaymentAuthorized = 20,
    Confirmed = 30,
    Shipped = 40,
    Delivered = 50,
    Cancelled = 90,
    RefundRequested = 91
}