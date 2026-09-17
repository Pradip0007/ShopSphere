using Microsoft.EntityFrameworkCore;
using ShopSphere.IntegrationTests.Common;

namespace ShopSphere.IntegrationTests.Orders;

[Collection("Containers")]
public sealed class OrderSagaHappyPathTests(ContainerFixture containers)
    : OrderSagaTestBase(containers, paymentSucceeds: true)
{
    [Fact]
    public async Task Full_saga_should_confirm_order_and_reserve_stock()
    {
        var placed = await PlaceOrderAsync("SKU-SAGA-1", 9.99m, 10, 2);

        var final = await PollOrderAsync(placed.OrderId, placed.UserId, "Confirmed");

        final.Status.Should().Be("Confirmed");
        final.TotalAmount.Should().Be(19.98m);
        final.Currency.Should().Be("USD");
        Factory.Payment.AuthorizeCallCount.Should().Be(1);
        Factory.Payment.IdempotencyKeys.Should()
            .ContainSingle(key => key == $"authorize:{placed.OrderId:D}");

        await UsingScope(async db =>
        {
            var stock = (await db.StockLevels.ToListAsync())
                .Single(level => level.Sku.Value == "SKU-SAGA-1");
            stock.Available.Should().Be(8);
            stock.Reserved.Should().Be(2);
        });
    }
}
