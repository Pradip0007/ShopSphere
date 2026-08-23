namespace ShopSphere.Api.Contracts.Events;

public sealed record PaymentFailed(
    Guid OrderId,
    string Reason,
    DateTimeOffset FailedAtUtc);