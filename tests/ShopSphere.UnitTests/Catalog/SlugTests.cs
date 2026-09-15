using ShopSphere.Domain.Catalog;

namespace ShopSphere.UnitTests.Catalog;

public sealed class SlugTests
{
    [Fact]
    public void From_should_reject_empty_string()
    {
        var act = () => Slug.From("");

        act.Should()
            .Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    public void From_should_reject_whitespace_only(string value)
    {
        var act = () => Slug.From(value);

        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("valid-slug")]
    [InlineData("abc123")]
    public void From_should_accept_slug_shaped_values(string value)
    {
        var slug = Slug.From(value);

        slug.Value.Should().Be(value);
        slug.ToString().Should().Be(value);
    }

    [Theory]
    [InlineData("Invalid-Slug")]
    [InlineData("two--hyphens")]
    [InlineData("-leading")]
    [InlineData("trailing-")]
    [InlineData("with space")]
    public void From_should_reject_values_that_are_not_slug_shaped(string value)
    {
        var act = () => Slug.From(value);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void FromName_should_normalize_text_to_kebab_case()
    {
        var slug = Slug.FromName("  Hello__World, Again!  ");

        slug.Value.Should().Be("hello-world-again");
    }

    [Fact]
    public void FromName_should_reject_names_without_slug_characters()
    {
        var act = () => Slug.FromName("___!!!");

        act.Should().Throw<ArgumentException>().WithMessage("*Could not derive a slug*");
    }

    [Fact]
    public void FromName_should_reject_null_name()
    {
        var act = () => Slug.FromName(null!);

        act.Should().Throw<ArgumentNullException>();
    }
}