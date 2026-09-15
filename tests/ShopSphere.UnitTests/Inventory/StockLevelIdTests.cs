using ShopSphere.Domain.Inventory;

namespace ShopSphere.UnitTests.Inventory;

public sealed class StockLevelIdTests
{
    [Fact]
    public void New_should_create_non_empty_id()
    {
        var id = StockLevelId.New();

        id.Value.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void ToString_should_return_guid_value()
    {
        var value = Guid.NewGuid();
        var id = new StockLevelId(value);

        id.ToString().Should().Be(value.ToString());
    }
}