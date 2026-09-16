using System.Net;
using System.Net.Http.Json;
using ShopSphere.IntegrationTests.Common;
using ShopSphere.IntegrationTests.Infrastructure;

namespace ShopSphere.IntegrationTests.Catalog;

[Collection("Containers")]
public sealed class ListProductsTests(ContainerFixture containers)
    : IntegrationTestBase(containers)
{
    [Fact]
    public async Task Anonymous_can_list_published_products_with_paging()
    {
        var categoryId = await UsingScope(async db => (await CatalogSeed.AddCategory(db)).Id);
        await UsingScope(db => CatalogSeed.SeedManyPublished(db, 15, categoryId));

        var response = await Client.GetAsync(
            "/api/v1/products?page=1&pageSize=10&sort=price_asc");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ProductListResponse>();
        body.Should().NotBeNull();
        body!.TotalCount.Should().Be(15);
        body.Page.Should().Be(1);
        body.PageSize.Should().Be(10);
        body.Items.Should().HaveCount(10);
        body.Items[0].Sku.Should().Be("SKU-0000");
    }

    [Fact]
    public async Task Paging_should_return_second_page()
    {
        var categoryId = await UsingScope(async db => (await CatalogSeed.AddCategory(db)).Id);
        await UsingScope(db => CatalogSeed.SeedManyPublished(db, 15, categoryId));

        var response = await Client.GetAsync(
            "/api/v1/products?page=2&pageSize=10&sort=price_asc");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ProductListResponse>();
        body!.Items.Should().HaveCount(5);
        body.Items[0].Sku.Should().Be("SKU-0010");
    }

    [Fact]
    public async Task List_should_apply_category_and_price_filters()
    {
        var matchingCategory = await UsingScope(async db => (await CatalogSeed.AddCategory(db, "Matching")).Id);
        var otherCategory = await UsingScope(async db => (await CatalogSeed.AddCategory(db, "Other")).Id);
        await UsingScope(async db =>
        {
            await CatalogSeed.AddPublishedProduct(db, "FILTER-1", "Low", 5m, matchingCategory);
            await CatalogSeed.AddPublishedProduct(db, "FILTER-2", "High", 25m, matchingCategory);
            await CatalogSeed.AddPublishedProduct(db, "FILTER-3", "Other", 10m, otherCategory);
        });

        var response = await Client.GetAsync(
            $"/api/v1/products?categoryId={matchingCategory.Value}&minPrice=10&maxPrice=30");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ProductListResponse>();
        body!.TotalCount.Should().Be(1);
        body.Items.Single().Sku.Should().Be("FILTER-2");
    }
}
