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
        HttpClient httpClient,
        IOptions<StripeOptions> options,
        ILogger<StripePaymentGateway> logger)
    {
        var stripeClient = new StripeClient(
            apiKey: options.Value.SecretKey,
            httpClient: new SystemNetHttpClient(httpClient));

        _intents = new PaymentIntentService(stripeClient);
        _logger = logger;
    }

    public async Task<AuthorizationResult> AuthorizeAsync(
        Money amount,
        string paymentMethodId,
        string idempotencyKey,
        IReadOnlyDictionary<string, string>? metadata,
        CancellationToken ct = default)
    {
        var opts = new PaymentIntentCreateOptions
        {
            Amount = ToStripeMinorUnits(amount),
            Currency = amount.Currency.ToLowerInvariant(),
            PaymentMethod = paymentMethodId,
            Confirm = true,
            CaptureMethod = "manual",
            AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
            {
                Enabled = true,
                AllowRedirects = "never"
            },
            Metadata = metadata is null
                ? null
                : new Dictionary<string, string>(metadata)
        };

        var reqOptions = new RequestOptions
        {
            IdempotencyKey = idempotencyKey
        };

        try
        {
            var intent = await _intents.CreateAsync(
                opts,
                reqOptions,
                ct);

            if (intent.Status is "requires_capture" or "succeeded")
            {
                return new AuthorizationResult(
                    true,
                    intent.Id,
                    null);
            }

            return new AuthorizationResult(
                false,
                intent.Id,
                $"Stripe status '{intent.Status}'.");
        }
        catch (StripeException ex)
        {
            _logger.LogWarning(
                ex,
                "Stripe authorize failed | code={Code}",
                ex.StripeError?.Code);

            return new AuthorizationResult(
                false,
                ex.StripeError?.PaymentIntent?.Id,
                ex.StripeError?.Message ?? ex.Message);
        }
    }

    private static long ToStripeMinorUnits(Money amount) =>
        (long)Math.Round(
            amount.Amount * 100m,
            MidpointRounding.ToEven);
}