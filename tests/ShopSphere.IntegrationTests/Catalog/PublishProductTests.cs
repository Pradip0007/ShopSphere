using System.Net;
using Microsoft.EntityFrameworkCore;
using ShopSphere.Domain.Catalog;
using ShopSphere.IntegrationTests.Common;
using ShopSphere.IntegrationTests.Infrastructure;

namespace ShopSphere.IntegrationTests.Catalog;

[Collection("Containers")]
public sealed class PublishProductTests(ContainerFixture containers)
    : IntegrationTestBase(containers)
{
    [Fact]
    public async Task Admin_publish_should_transition_draft_to_published()
    {
        var productId = await UsingScope(async db =>
        {
            var category = await CatalogSeed.AddCategory(db);
            return (await CatalogSeed.AddDraftProduct(
                db, category.Id, "SKU-P1", "Draft Product")).Id.Value;
        });

        var response = await Client.WithAdmin().PostAsync(
            $"/api/v1/admin/products/{productId}/publish", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        await UsingScope(async db =>
        {
            var product = await db.Products.SingleAsync(p => p.Id == new ProductId(productId));
            product.Status.Should().Be(ProductStatus.Published);
        });
    }

    [Fact]
    public async Task Customer_cannot_publish_product()
    {
        var productId = await UsingScope(async db =>
        {
            var category = await CatalogSeed.AddCategory(db);
            return (await CatalogSeed.AddDraftProduct(
                db, category.Id, "SKU-P2", "Draft Product")).Id.Value;
        });

        var response = await Client.WithCustomer().PostAsync(
            $"/api/v1/admin/products/{productId}/publish", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
