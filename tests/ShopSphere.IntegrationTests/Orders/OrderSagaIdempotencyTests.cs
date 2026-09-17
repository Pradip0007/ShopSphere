using System.Net;
using System.Net.Http.Json;
using ShopSphere.IntegrationTests.Common;
using ShopSphere.IntegrationTests.Infrastructure;

namespace ShopSphere.IntegrationTests.Orders;

[Collection("Containers")]
public sealed class OrderSagaIdempotencyTests(ContainerFixture containers)
    : OrderSagaTestBase(containers, paymentSucceeds: true)
{
    [Fact]
    public async Task Same_idempotency_key_should_return_the_same_order_without_second_authorization()
    {
        var seed = await UsingScope(db =>
            OrderSagaSeed.SeedCustomerAndProduct(db, "SKU-IDEM", 5m, 10));
        var client = Client.WithCustomer(seed.UserId);
        client.DefaultRequestHeaders.Add("Idempotency-Key", "checkout-replay-1");

        var cartResponse = await client.PostAsJsonAsync(
            "/api/v1/cart/items",
            new { productId = seed.ProductId, quantity = 1 });
        cartResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var request = new CheckoutRequest(
        new ShippingAddressRequest("1 Main Street", null, "Test City", "12345", "US"));
        var first = await client.PostAsJsonAsync("/api/v1/checkout", request);
        var second = await client.PostAsJsonAsync("/api/v1/checkout", request);

        var firstText = await first.Content.ReadAsStringAsync();
        var secondText = await second.Content.ReadAsStringAsync();

        var firstBody = await first.Content.ReadFromJsonAsync<CheckoutResponse>();
        var secondBody = await second.Content.ReadFromJsonAsync<CheckoutResponse>();

        first.StatusCode.Should().Be(HttpStatusCode.Created, firstText);
second.StatusCode.Should().Be(HttpStatusCode.Created, secondText);
secondBody!.OrderId.Should().Be(firstBody!.OrderId);

await Poll.UntilAsync(
    () => Task.FromResult(Factory.Payment.AuthorizeCallCount),
    count => count == 1,
    TimeSpan.FromSeconds(5),
    TimeSpan.FromMilliseconds(100),
    "payment authorization");
    }
}
