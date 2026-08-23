using System.ComponentModel.DataAnnotations;

namespace ShopSphere.Api.Infrastructure.Payments;

public sealed class StripeOptions
{
    public const string SectionName = "Stripe";

    [Required]
    public string SecretKey { get; init; } = default!;

    public string? WebhookSecret { get; init; }

    public int TimeoutSeconds { get; init; } = 30;
}