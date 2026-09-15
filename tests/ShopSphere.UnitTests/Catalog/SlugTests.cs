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
}