using System.Net;
using System.Net.Http.Json;
using ShopSphere.IntegrationTests.Common;
using ShopSphere.IntegrationTests.Infrastructure;

namespace ShopSphere.IntegrationTests.Auth;

[Collection("Containers")]
public sealed class AuthorizationTests(ContainerFixture containers)
    : IntegrationTestBase(containers)
{
    [Fact]
    public async Task Anonymous_request_to_me_should_return_unauthorized()
    {
        var response = await Client.GetAsync("/api/v1/me");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task WithAuth_request_to_me_should_return_success()
    {
        var response = await Client
            .WithAuth(email: "customer@example.com")
            .GetAsync("/api/v1/me");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Admin_should_be_authorized_for_admin_product_endpoint()
    {
        var response = await Client
            .WithAdmin(email: "admin@example.com")
            .PostAsJsonAsync(
                "/api/v1/admin/products",
                new
                {
                    title = "Authorization test product",
                    sku = "AUTH-ADMIN-1",
                    description = "Authorization test",
                    price = 10m,
                    currency = "USD",
                    initialStock = 0,
                    categoryId = Guid.NewGuid()
                });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Customer_should_be_forbidden_from_admin_product_endpoint()
    {
        var response = await Client
            .WithCustomer(email: "customer@example.com")
            .PostAsJsonAsync(
                "/api/v1/admin/products",
                new
                {
                    title = "Authorization test product",
                    sku = "AUTH-CUSTOMER-1",
                    description = "Authorization test",
                    price = 10m,
                    currency = "USD",
                    initialStock = 0,
                    categoryId = Guid.NewGuid()
                });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
