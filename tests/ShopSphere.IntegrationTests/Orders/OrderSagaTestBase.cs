using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ShopSphere.Infrastructure.Persistence;
using ShopSphere.IntegrationTests.Common;
using ShopSphere.IntegrationTests.Infrastructure;

namespace ShopSphere.IntegrationTests.Orders;

public abstract class OrderSagaTestBase(
    ContainerFixture containers,
    bool paymentSucceeds) : IAsyncLifetime
{
    protected OrderSagaFactory Factory { get; private set; } = null!;
    protected HttpClient Client { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        Factory = new OrderSagaFactory(containers, paymentSucceeds);
        Client = Factory.CreateClient();
        await Factory.ResetDatabaseAsync();
    }

    protected async Task<T> UsingScope<T>(Func<ShopSphereDbContext, Task<T>> action)
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ShopSphereDbContext>();
        return await action(db);
    }

    protected async Task UsingScope(Func<ShopSphereDbContext, Task> action)
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ShopSphereDbContext>();
        await action(db);
    }

    protected async Task<(Guid OrderId, Guid UserId, Guid ProductId)> PlaceOrderAsync(
        string sku,
        decimal price,
        int stockQuantity,
        int quantity)
    {
        var seed = await UsingScope(db =>
            OrderSagaSeed.SeedCustomerAndProduct(db, sku, price, stockQuantity));
        var client = Client.WithCustomer(seed.UserId);
        var cartResponse = await client.PostAsJsonAsync(
            "/api/v1/cart/items",
            new { productId = seed.ProductId, quantity });
        cartResponse.IsSuccessStatusCode.Should().BeTrue();

        var response = await client.PostAsJsonAsync(
            "/api/v1/checkout",
            new CheckoutRequest(
                new ShippingAddressRequest(
                    "1 Main Street",
                    null,
                    "Test City",
                    "12345",
                    "US")));
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<CheckoutResponse>();
        body.Should().NotBeNull();
        return (body!.OrderId, seed.UserId, seed.ProductId);
    }

    protected async Task<OrderResponse?> ReadOrderAsync(Guid orderId, Guid userId)
    {
        var response = await Client.WithCustomer(userId)
            .GetAsync($"/api/v1/orders/{orderId}");
        if (response.StatusCode != HttpStatusCode.OK)
            return null;
        return await response.Content.ReadFromJsonAsync<OrderResponse>();
    }

    protected Task<OrderResponse> PollOrderAsync(
        Guid orderId,
        Guid userId,
        string status) =>
        Poll.UntilAsync(
            () => ReadOrderAsync(orderId, userId),
            order => order?.Status == status,
            TimeSpan.FromSeconds(30),
            TimeSpan.FromMilliseconds(250),
            $"Order.Status == {status}");

    public async Task DisposeAsync()
    {
        Client.Dispose();
        await Factory.DisposeAsync();
    }
}
