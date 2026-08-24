using Microsoft.Extensions.Options;
using ShopSphere.Domain.Common;
using ShopSphere.Domain.Payments;
using Stripe;

namespace ShopSphere.Api.Infrastructure.Payments;

public sealed class StripePaymentGateway : IPaymentGateway
{
    private readonly PaymentIntentService _intents;
    private readonly ILogger<StripePaymentGateway> _logger;

    public StripePaymentGateway(
        IOptions<StripeOptions> options,
        ILogger<StripePaymentGateway> logger)
    {
        StripeConfiguration.ApiKey = options.Value.SecretKey;
        _intents = new PaymentIntentService();
        _logger = logger;
    }

    public async Task<AuthorizationResult> AuthorizeAsync(
        Money amount,
        string paymentMethodId,
        string idempotencyKey,
        IReadOnlyDictionary<string, string>? metadata,
        CancellationToken ct = default)
    {
        var options = new PaymentIntentCreateOptions
        {
            Amount = ToStripeMinorUnits(amount),
            Currency = amount.Currency.ToLowerInvariant(),
            PaymentMethod = paymentMethodId,
            Confirm = true,               // authorize immediately
            CaptureMethod = "manual",     // auth-then-capture; capture on shipment
            AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
            {
                Enabled = true,
                AllowRedirects = "never"  // no 3DS redirect flow for the saga
            },
            Metadata = new Dictionary<string, string>
            {
                ["idempotencyKey"] = idempotencyKey
            }
        };

        var requestOptions = new RequestOptions
        {
            IdempotencyKey = idempotencyKey
        };

        try
        {
            var intent = await _intents.CreateAsync(options, requestOptions, ct);

            if (intent.Status == "requires_capture" || intent.Status == "succeeded")
            {
                return new AuthorizationResult(true, intent.Id, null);
            }

            return new AuthorizationResult(false, intent.Id,
                $"Stripe returned status '{intent.Status}'.");
        }
        catch (StripeException ex)
        {
            _logger.LogWarning(ex,
                "Stripe authorize failed | code={Code} message={Message}",
                ex.StripeError?.Code, ex.Message);
            return new AuthorizationResult(false, ex.StripeError?.PaymentIntent?.Id,
                ex.StripeError?.Message ?? ex.Message);
        }
    }

    private static long ToStripeMinorUnits(Money amount)
    {
        // Stripe uses cents. Multiply by 100 and truncate.
        return (long)Math.Round(amount.Amount * 100m, MidpointRounding.ToEven);
    }
}