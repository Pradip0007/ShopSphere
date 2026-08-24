namespace ShopSphere.Api.Infrastructure.Payments;

public interface IProcessedWebhookStore
{
    /// <summary>
    /// Returns true if the event id had not been seen. Atomically records it.
    /// </summary>
    Task<bool> TryMarkAsync(string stripeEventId, CancellationToken ct = default);
}