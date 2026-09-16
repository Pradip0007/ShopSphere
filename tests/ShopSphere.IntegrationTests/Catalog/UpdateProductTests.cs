using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using ShopSphere.Domain.Catalog;
using ShopSphere.Domain.Common;
using ShopSphere.IntegrationTests.Common;
using ShopSphere.IntegrationTests.Infrastructure;

namespace ShopSphere.IntegrationTests.Catalog;

[Collection("Containers")]
public sealed class UpdateProductTests(ContainerFixture containers)
    : IntegrationTestBase(containers)
{
    [Fact]
    public async Task Admin_can_update_product_and_persist_changes()
    {
        var productId = await UsingScope(async db =>
        {
            var category = await CatalogSeed.AddCategory(db);
            return (await CatalogSeed.AddPublishedProduct(
                db, "SKU-U1", "Old", 5m, category.Id)).Id.Value;
        });

        var request = new UpdateProductRequest("New Name", "New Desc", 7.50m, "EUR");
        var response = await Client.WithAdmin().PutAsJsonAsync(
            $"/api/v1/admin/products/{productId}", request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        await UsingScope(async db =>
        {
            var product = await db.Products.SingleAsync(p => p.Id == new ProductId(productId));
            product.Title.Should().Be("New Name");
            product.Slug.Value.Should().Be("new-name");
            product.Description.Should().Be("New Desc");
            product.Price.Should().Be(new Money(7.50m, "EUR"));
        });
    }

    [Fact]
    public async Task Update_nonexistent_product_should_return_not_found()
    {
        var response = await Client.WithAdmin().PutAsJsonAsync(
            $"/api/v1/admin/products/{Guid.NewGuid()}",
            new UpdateProductRequest("Name", "Description", 1m, "USD"));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
