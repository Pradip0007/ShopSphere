namespace ShopSphere.Api.Infrastructure.Messaging;

public interface IProcessedMessageStore
{
    Task<bool> TryMarkAsync(
        Guid messageId,
        string consumerName,
        CancellationToken ct = default);
}