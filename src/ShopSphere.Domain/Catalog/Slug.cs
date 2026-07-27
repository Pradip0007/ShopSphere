using System.Text.RegularExpressions;

namespace ShopSphere.Domain.Catalog;

/// <summary>
/// URL-safe kebab-case identifier. Regex: ^[a-z0-9]+(?:-[a-z0-9]+)*$
/// </summary>
public sealed partial record Slug
{
    [GeneratedRegex(@"^[a-z0-9]+(?:-[a-z0-9]+)*$", RegexOptions.CultureInvariant)]
    private static partial Regex SlugPattern();

    public string Value { get; }

    private Slug(string value) => Value = value;

    /// <summary>
    /// Parses a raw string that is already slug-shaped. Throws on invalid input.
    /// Use FromName for user-friendly conversion of arbitrary text.
    /// </summary>
    public static Slug From(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        if (!SlugPattern().IsMatch(value))
            throw new ArgumentException($"'{value}' is not a valid slug.", nameof(value));
        return new Slug(value);
    }

    /// <summary>
    /// Best-effort factory: lowercases, replaces whitespace/underscores with hyphens,
    /// strips other punctuation, collapses runs of hyphens, trims edges.
    /// </summary>
    public static Slug FromName(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var lowered = name.Trim().ToLowerInvariant();
        var hyphenated = WhitespaceOrUnderscore().Replace(lowered, "-");
        var stripped = NonSlugChar().Replace(hyphenated, string.Empty);
        var collapsed = MultiHyphen().Replace(stripped, "-").Trim('-');

        if (collapsed.Length == 0)
            throw new ArgumentException($"Could not derive a slug from '{name}'.", nameof(name));

        return new Slug(collapsed);
    }

    public override string ToString() => Value;

    [GeneratedRegex(@"[\s_]+", RegexOptions.CultureInvariant)]
    private static partial Regex WhitespaceOrUnderscore();

    [GeneratedRegex(@"[^a-z0-9-]", RegexOptions.CultureInvariant)]
    private static partial Regex NonSlugChar();

    [GeneratedRegex(@"-{2,}", RegexOptions.CultureInvariant)]
    private static partial Regex MultiHyphen();
}