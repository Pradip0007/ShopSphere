using ShopSphere.Domain.Inventory;

namespace ShopSphere.UnitTests.Inventory;

public sealed class InventoryErrorsTests
{
    [Fact]
    public void QuantityNotPositive_should_create_validation_error()
    {
        var error = InventoryErrors.QuantityNotPositive("Reserve");

        error.Code.Should().Be("inventory.quantity_not_positive");
        error.Message.Should().Be("Reserve quantity must be a positive integer.");
    }

    [Fact]
    public void InsufficientStock_should_create_conflict_error()
    {
        var error = InventoryErrors.InsufficientStock(10, 4);

        error.Code.Should().Be("inventory.insufficient_stock");
        error.Message.Should().Be("Insufficient stock: requested 10, available 4.");
    }

    [Fact]
    public void InsufficientReserved_should_create_conflict_error()
    {
        var error = InventoryErrors.InsufficientReserved(5, 2);

        error.Code.Should().Be("inventory.insufficient_reserved");
        error.Message.Should().Be("Cannot release 5 units — only 2 reserved.");
    }

    [Fact]
    public void AdjustmentBelowZero_should_create_conflict_error()
    {
        var error = InventoryErrors.AdjustmentBelowZero(3, -5);

        error.Code.Should().Be("inventory.adjustment_below_zero");
        error.Message.Should()
            .Be("Adjustment would drive Available below zero (was 3, delta -5).");
    }
}