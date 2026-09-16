using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using ShopSphere.Domain.Catalog;
using ShopSphere.IntegrationTests.Common;
using ShopSphere.IntegrationTests.Infrastructure;

namespace ShopSphere.IntegrationTests.Catalog;

[Collection("Containers")]
public sealed class DeleteProductTests(ContainerFixture containers)
    : IntegrationTestBase(containers)
{
    [Fact]
    public async Task Admin_delete_should_archive_not_hard_delete()
    {
        var productId = await UsingScope(async db =>
        {
            var category = await CatalogSeed.AddCategory(db);
            return (await CatalogSeed.AddPublishedProduct(
                db, "SKU-D1", categoryId: category.Id)).Id.Value;
        });

        var response = await Client.WithAdmin().DeleteAsync(
            $"/api/v1/admin/products/{productId}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        await UsingScope(async db =>
        {
            var product = await db.Products.SingleAsync(p => p.Id == new ProductId(productId));
            product.Status.Should().Be(ProductStatus.Archived);
        });
    }

    [Fact]
    public async Task Archived_product_should_not_appear_in_public_list()
    {
        var ids = await UsingScope(async db =>
        {
            var category = await CatalogSeed.AddCategory(db);
            var archived = await CatalogSeed.AddPublishedProduct(
                db, "SKU-A1", "Archived Widget", categoryId: category.Id);
            var visible = await CatalogSeed.AddPublishedProduct(
                db, "SKU-A2", "Visible Widget", categoryId: category.Id);
            return (archived.Id.Value, visible.Id.Value);
        });

        var deleteResponse = await Client.WithAdmin().DeleteAsync(
            $"/api/v1/admin/products/{ids.Item1}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var response = await Client.GetAsync("/api/v1/products");
        var body = await response.Content.ReadFromJsonAsync<ProductListResponse>();

        body!.Items.Should().ContainSingle();
        body.Items.Single().Sku.Should().Be("SKU-A2");
    }
}
