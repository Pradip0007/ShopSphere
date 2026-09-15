using ShopSphere.Domain.Inventory;

namespace ShopSphere.UnitTests.Inventory;

public sealed class StockReservationTests
{
    [Fact]
    public void Constructor_should_set_properties()
    {
        var orderId = Guid.NewGuid();
        var reservedAt = DateTimeOffset.UtcNow;

        var reservation = new StockReservation(
            orderId,
            "SKU-123",
            3,
            reservedAt);

        reservation.OrderId.Should().Be(orderId);
        reservation.Sku.Should().Be("SKU-123");
        reservation.Quantity.Should().Be(3);
        reservation.ReservedAt.Should().Be(reservedAt);
    }

    [Fact]
    public void Constructor_should_reject_empty_order_id()
    {
        var act = () => new StockReservation(
            Guid.Empty,
            "SKU-123",
            1,
            DateTimeOffset.UtcNow);

        act.Should()
            .Throw<ArgumentException>()
            .WithMessage("orderId");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    public void Constructor_should_reject_empty_sku(string sku)
    {
        var act = () => new StockReservation(
            Guid.NewGuid(),
            sku,
            1,
            DateTimeOffset.UtcNow);

        act.Should()
            .Throw<ArgumentException>()
            .WithMessage("sku");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_should_reject_non_positive_quantity(int quantity)
    {
        var act = () => new StockReservation(
            Guid.NewGuid(),
            "SKU-123",
            quantity,
            DateTimeOffset.UtcNow);

        act.Should()
            .Throw<ArgumentException>()
            .WithMessage("quantity");
    }
}