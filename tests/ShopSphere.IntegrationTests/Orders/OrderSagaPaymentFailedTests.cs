using Microsoft.EntityFrameworkCore;
using ShopSphere.IntegrationTests.Common;

namespace ShopSphere.IntegrationTests.Orders;

[Collection("Containers")]
public sealed class OrderSagaPaymentFailedTests(ContainerFixture containers)
    : OrderSagaTestBase(containers, paymentSucceeds: false)
{
    [Fact]
    public async Task Payment_failure_should_release_reserved_stock_and_mark_order_failed()
    {
        var placed = await PlaceOrderAsync("SKU-SAGA-2", 5m, 10, 3);

        var final = await PollOrderAsync(placed.OrderId, placed.UserId, "PaymentFailed");

        final.Status.Should().Be("PaymentFailed");
        Factory.Payment.AuthorizeCallCount.Should().Be(1);

        await UsingScope(async db =>
        {
            var stock = (await db.StockLevels.ToListAsync())
                .Single(level => level.Sku.Value == "SKU-SAGA-2");
            stock.Available.Should().Be(10);
            stock.Reserved.Should().Be(0);
        });
    }
}
