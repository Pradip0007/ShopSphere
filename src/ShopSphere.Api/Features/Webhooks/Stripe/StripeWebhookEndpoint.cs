using MassTransit;
using Microsoft.Extensions.Options;
using ShopSphere.Api.Contracts.Events;
using ShopSphere.Api.Infrastructure.Payments;
using Stripe;

namespace ShopSphere.Api.Features.Webhooks.Stripe;

public static class StripeWebhookEndpoint
{
    public static IEndpointRouteBuilder MapStripeWebhook(this IEndpointRouteBuilder routes)
    {
        routes.MapPost("/api/v1/webhooks/stripe", HandleAsync)
            .WithTags("Webhooks")
            .AllowAnonymous();

        return routes;
    }

    private static async Task<IResult> HandleAsync(
        HttpContext http,
        IOptions<StripeOptions> options,
        IProcessedWebhookStore processed,
        IBus bus,
        ILoggerFactory loggerFactory,
        CancellationToken ct)
    {
        var log = loggerFactory.CreateLogger("StripeWebhook");
        var webhookSecret = options.Value.WebhookSecret;
        if (string.IsNullOrWhiteSpace(webhookSecret))
        {
            log.LogError("Stripe:WebhookSecret is not configured.");
            return Results.Problem(statusCode: StatusCodes.Status500InternalServerError);
        }

        string body;
        using (var reader = new StreamReader(http.Request.Body))
        {
            body = await reader.ReadToEndAsync(ct);
        }

        global::Stripe.Event stripeEvent;
        try
        {
            stripeEvent = EventUtility.ConstructEvent(
                body,
                http.Request.Headers["Stripe-Signature"],
                webhookSecret,
                tolerance: 300,           // seconds — default 5min
                throwOnApiVersionMismatch: false);
        }
        catch (StripeException ex)
        {
            log.LogWarning(ex, "Stripe signature verification failed.");
            return Results.BadRequest(new { error = "Invalid signature." });
        }

        if (!await processed.TryMarkAsync(stripeEvent.Id, ct))
        {
            log.LogInformation("Duplicate Stripe event {EventId} — skipping.", stripeEvent.Id);
            return Results.Ok(new { received = true, duplicate = true });
        }

        switch (stripeEvent.Type)
        {
            case "payment_intent.succeeded":
                if (stripeEvent.Data.Object is PaymentIntent pi)
                {
                    await OnSucceededAsync(pi, bus, log, ct);
                }
                break;

            case "payment_intent.payment_failed":
                if (stripeEvent.Data.Object is PaymentIntent piFailed)
                {
                    await OnFailedAsync(piFailed, bus, log, ct);
                }
                break;

            default:
                log.LogInformation("Unhandled Stripe event type {Type}", stripeEvent.Type);
                break;
        }

        return Results.Ok(new { received = true });
    }

    private static async Task OnSucceededAsync(
        PaymentIntent pi,
        IBus bus,
        ILogger log,
        CancellationToken ct)
    {
        var orderId = ExtractOrderId(pi);
        if (orderId == Guid.Empty)
        {
            log.LogWarning("payment_intent.succeeded without orderId metadata piId={PI}", pi.Id);
            return;
        }

        var currency = pi.Currency.ToUpperInvariant();
        var amount = pi.Amount / 100m;

        await bus.Publish(new PaymentCaptured(orderId, pi.Id, amount, currency, DateTimeOffset.UtcNow), ct);
    }

    private static async Task OnFailedAsync(
        PaymentIntent pi,
        IBus bus,
        ILogger log,
        CancellationToken ct)
    {
        var orderId = ExtractOrderId(pi);
        if (orderId == Guid.Empty)
        {
            log.LogWarning("payment_intent.payment_failed without orderId metadata piId={PI}", pi.Id);
            return;
        }

        var reason = pi.LastPaymentError?.Message ?? "Unknown Stripe decline";
        await bus.Publish(new PaymentFailed(orderId, reason, DateTimeOffset.UtcNow), ct);
    }

    private static Guid ExtractOrderId(PaymentIntent pi)
    {
        if (pi.Metadata is null) return Guid.Empty;
        if (!pi.Metadata.TryGetValue("orderId", out var raw)) return Guid.Empty;
        return Guid.TryParse(raw, out var g) ? g : Guid.Empty;
    }
}