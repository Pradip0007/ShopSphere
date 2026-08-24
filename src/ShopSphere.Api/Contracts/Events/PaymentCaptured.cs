namespace ShopSphere.Api.Contracts.Events;

public sealed record PaymentCaptured(
    Guid OrderId,
    string PaymentIntentId,
    decimal Amount,
    string Currency,
    DateTimeOffset CapturedAtUtc);