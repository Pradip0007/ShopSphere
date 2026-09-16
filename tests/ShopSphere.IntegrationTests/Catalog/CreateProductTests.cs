using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using ShopSphere.Domain.Catalog;
using ShopSphere.Domain.Common;
using ShopSphere.IntegrationTests.Common;
using ShopSphere.IntegrationTests.Infrastructure;

namespace ShopSphere.IntegrationTests.Catalog;

[Collection("Containers")]
public sealed class CreateProductTests(ContainerFixture containers)
    : IntegrationTestBase(containers)
{
    [Fact]
    public async Task Admin_can_create_product_and_persist_product_and_stock()
    {
        var categoryId = await UsingScope(async db => (await CatalogSeed.AddCategory(db)).Id.Value);
        var request = new CreateProductRequest(
            "Fresh Widget", "SKU-NEW", "Shiny", 19.99m, "USD", 25, categoryId);

        var response = await Client.WithAdmin().PostAsJsonAsync(
            "/api/v1/admin/products", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<CreateProductResponse>();
        body.Should().NotBeNull();

        await UsingScope(async db =>
        {
            var saved = await db.Products.SingleAsync(product => product.Id == new ProductId(body!.Id));
            saved.Title.Should().Be("Fresh Widget");
            saved.Sku.Value.Should().Be("SKU-NEW");
            saved.Description.Should().Be("Shiny");
            saved.Price.Should().Be(new Money(19.99m, "USD"));
            saved.Status.Should().Be(ProductStatus.Published);

            var stock = await db.StockLevels.SingleAsync(level => level.ProductId == saved.Id);
            stock.Available.Should().Be(25);
            stock.Reserved.Should().Be(0);
        });
    }

    [Fact]
    public async Task Customer_cannot_create_product()
    {
        var request = new CreateProductRequest(
            "Customer Widget", "SKU-CUSTOMER", "No", 1m, "USD", 0, Guid.NewGuid());

        var response = await Client.WithCustomer().PostAsJsonAsync(
            "/api/v1/admin/products", request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Duplicate_sku_should_return_conflict()
    {
        var categoryId = await UsingScope(async db =>
            (await CatalogSeed.AddCategory(db)).Id.Value);
        await UsingScope(async db =>
        {
            await CatalogSeed.AddPublishedProduct(
                db, "SKU-DUP", categoryId: new(categoryId));
        });

        await UsingScope(async db =>
        {
            var existingSkus = await db.Products
                .Select(product => product.Sku)
                .ToListAsync();
            existingSkus.Should().Contain(Sku.From("SKU-DUP"));
        });

        var request = new CreateProductRequest(
            "Another", "SKU-DUP", "Duplicate", 1m, "USD", 0, categoryId);
        var response = await Client.WithAdmin().PostAsJsonAsync(
            "/api/v1/admin/products", request);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
}
