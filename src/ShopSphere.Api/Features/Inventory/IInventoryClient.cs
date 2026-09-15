namespace ShopSphere.Api.Features.Inventory;

public interface IInventoryClient
{
    Task<ReserveResult> ReserveAsync(
        Guid orderId,
        string sku,
        int quantity,
        CancellationToken ct);

    Task<ReleaseResult> ReleaseAsync(
        Guid orderId,
        string sku,
        int quantity,
        CancellationToken ct);
}

public sealed record ReserveResult(
    bool Success,
    int AvailableAfter,
    ReserveFailReason? FailReason,
    string? Message);

public sealed record ReleaseResult(
    bool Success,
    int AvailableAfter);

public enum ReserveFailReason
{
    None,
    UnknownSku,
    InsufficientStock,
    InvalidRequest,
    Unavailable
}