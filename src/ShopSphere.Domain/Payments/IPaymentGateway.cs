using ShopSphere.Domain.Common;

namespace ShopSphere.Domain.Payments;

public interface IPaymentGateway
{
    Task<AuthorizationResult> AuthorizeAsync(
        Money amount,
        string paymentMethodId,
        string idempotencyKey,
        CancellationToken ct = default);
}

public sealed record AuthorizationResult(
    bool Succeeded,
    string? PaymentIntentId,
    string? DeclineReason);