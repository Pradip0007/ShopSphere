namespace ShopSphere.Api.Contracts.Events;

public sealed record PaymentAuthorized(
    Guid OrderId,
    string PaymentIntentId,
    decimal Amount,
    string Currency,
    DateTimeOffset AuthorizedAtUtc);