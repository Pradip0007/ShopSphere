using ShopSphere.Domain.Common;

namespace ShopSphere.Domain.Ordering;

public sealed class Address : ValueObject
{
    public string Line1 { get; }
    public string? Line2 { get; }
    public string City { get; }
    public string PostalCode { get; }
    public string Country { get; }

    public Address(string line1, string? line2, string city, string postalCode, string country)
    {
        if (string.IsNullOrWhiteSpace(line1)) throw new ArgumentException("Line1 required.", nameof(line1));
        if (string.IsNullOrWhiteSpace(city)) throw new ArgumentException("City required.", nameof(city));
        if (string.IsNullOrWhiteSpace(postalCode)) throw new ArgumentException("PostalCode required.", nameof(postalCode));
        if (string.IsNullOrWhiteSpace(country)) throw new ArgumentException("Country required.", nameof(country));
        if (country.Length != 2) throw new ArgumentException("Country must be an ISO 3166 alpha-2 code.", nameof(country));

        Line1 = line1.Trim();
        Line2 = string.IsNullOrWhiteSpace(line2) ? null : line2.Trim();
        City = city.Trim();
        PostalCode = postalCode.Trim();
        Country = country.Trim().ToUpperInvariant();
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Line1;
        yield return Line2;
        yield return City;
        yield return PostalCode;
        yield return Country;
    }
}