using ShopSphere.Domain.Ordering;

namespace ShopSphere.UnitTests.Orders;

public sealed class AddressTests
{
    [Fact]
    public void Constructor_should_trim_and_normalize_address_fields()
    {
        var address = new Address("  123 Main St  ", "  Apt 2  ", "  City  ", " 700001 ", "in");

        address.Line1.Should().Be("123 Main St");
        address.Line2.Should().Be("Apt 2");
        address.City.Should().Be("City");
        address.PostalCode.Should().Be("700001");
        address.Country.Should().Be("IN");
    }

    [Fact]
    public void Constructor_should_turn_blank_line2_into_null()
    {
        var address = new Address("Line 1", "  ", "City", "12345", "US");

        address.Line2.Should().BeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_should_reject_missing_required_fields(string value)
    {
        var line1 = () => new Address(value, null, "City", "12345", "US");
        var city = () => new Address("Line 1", null, value, "12345", "US");
        var postalCode = () => new Address("Line 1", null, "City", value, "US");
        var country = () => new Address("Line 1", null, "City", "12345", value);

        line1.Should().Throw<ArgumentException>();
        city.Should().Throw<ArgumentException>();
        postalCode.Should().Throw<ArgumentException>();
        country.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("U")]
    [InlineData("USA")]
    public void Constructor_should_reject_non_iso_country_code(string country)
    {
        var act = () => new Address("Line 1", null, "City", "12345", country);

        act.Should().Throw<ArgumentException>().WithMessage("*ISO 3166*");
    }

    [Fact]
    public void Addresses_should_use_structural_value_equality()
    {
        var first = new Address("Line 1", "Line 2", "City", "12345", "US");
        var second = new Address(" Line 1 ", " Line 2 ", " City ", " 12345 ", "us");

        first.Should().Be(second);
    }
}
