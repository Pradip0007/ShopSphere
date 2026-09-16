using System.Net;
using System.Net.Http.Json;
using ShopSphere.IntegrationTests.Common;
using ShopSphere.IntegrationTests.Infrastructure;

namespace ShopSphere.IntegrationTests.Catalog;

[Collection("Containers")]
public sealed class GetProductBySlugTests(ContainerFixture containers)
    : IntegrationTestBase(containers)
{
    [Fact]
    public async Task Anonymous_can_get_published_product_detail_by_slug()
    {
        var product = await UsingScope(async db =>
        {
            var category = await CatalogSeed.AddCategory(db, "Awesome Category");
            return await CatalogSeed.AddPublishedProduct(
                db, "SKU-G1", "Awesome Thing", 12m, category.Id);
        });

        var response = await Client.GetAsync("/api/v1/products/awesome-thing");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ProductDetailResponse>();
        body.Should().NotBeNull();
        body!.Id.Should().Be(product.Id.Value);
        body.Sku.Should().Be("SKU-G1");
        body.Title.Should().Be("Awesome Thing");
        body.Category.Should().Be("Awesome Category");
        body.Stock.Should().Be(0);
        body.RatingCount.Should().Be(0);
    }

    [Fact]
    public async Task Missing_slug_should_return_not_found()
    {
        var response = await Client.GetAsync("/api/v1/products/by-slug/nope");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
