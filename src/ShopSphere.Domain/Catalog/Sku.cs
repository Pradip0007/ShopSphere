using System.Text.RegularExpressions;

namespace ShopSphere.Domain.Catalog;

/// <summary>
/// Stock Keeping Unit. Uppercase alphanumerics and hyphens, 3–32 chars.
/// </summary>
public sealed partial record Sku
{
    [GeneratedRegex(@"^[A-Z0-9]+(?:-[A-Z0-9]+)*$", RegexOptions.CultureInvariant)]
    private static partial Regex SkuPattern();

    public string Value { get; }

    private Sku(string value) => Value = value;

    public static Sku From(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        var normalised = value.Trim().ToUpperInvariant();
        if (normalised.Length < 3 || normalised.Length > 32)
            throw new ArgumentException("SKU must be between 3 and 32 characters.", nameof(value));
        if (!SkuPattern().IsMatch(normalised))
            throw new ArgumentException($"'{value}' is not a valid SKU.", nameof(value));
        return new Sku(normalised);
    }

    public override string ToString() => Value;
}