using System.Globalization;

namespace ShopSphere.Domain.Common;

/// <summary>
/// Immutable monetary amount in a specific ISO 4217 currency.
/// Arithmetic across currencies throws — conversion is a policy decision
/// that lives outside the value object.
/// </summary>
public sealed record Money
{
    public decimal Amount { get; }
    public string Currency { get; }

    public Money(decimal amount, string currency)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(currency);

        var normalised = currency.Trim().ToUpperInvariant();
        if (normalised.Length != 3 || !normalised.All(char.IsLetter))
            throw new ArgumentException(
                $"Currency must be a 3-letter ISO 4217 code (got '{currency}').",
                nameof(currency));

        Amount = amount;
        Currency = normalised;
    }

    /// <summary>Zero in the given currency — useful for accumulators.</summary>
    public static Money Zero(string currency) => new(0m, currency);

    public static Money operator +(Money left, Money right)
    {
        EnsureSameCurrency(left, right);
        return new Money(left.Amount + right.Amount, left.Currency);
    }

    public static Money operator -(Money left, Money right)
    {
        EnsureSameCurrency(left, right);
        return new Money(left.Amount - right.Amount, left.Currency);
    }

    public static Money operator *(Money left, decimal factor) =>
        new(left.Amount * factor, left.Currency);

    public static Money operator *(decimal factor, Money right) => right * factor;

    private static void EnsureSameCurrency(Money left, Money right)
    {
        if (left.Currency != right.Currency)
            throw new InvalidOperationException(
                $"Currency mismatch: {left.Currency} vs {right.Currency}.");
    }

    public override string ToString() =>
        string.Create(CultureInfo.InvariantCulture, $"{Amount:0.00} {Currency}");
}