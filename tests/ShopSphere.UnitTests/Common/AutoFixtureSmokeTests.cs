namespace ShopSphere.UnitTests.Common;

public sealed class AutoFixtureSmokeTests
{
    [Fact]
    public void Fixture_produces_non_default_values()
    {
        var fixture = new Fixture();

        var s = fixture.Create<string>();
        var i = fixture.Create<int>();

        s.Should().NotBeNullOrEmpty();
        i.Should().NotBe(0);
    }

    [Theory]
    [AutoData]
    public void AutoData_injects_arguments(string a, int b)
    {
        a.Should().NotBeNullOrEmpty();
        b.Should().NotBe(0);
    }
}