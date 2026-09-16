using Microsoft.EntityFrameworkCore;
using ShopSphere.Api.Features.Admin.DeleteProduct;
using ShopSphere.Api.Features.Admin.UpdateProduct;
using ShopSphere.Api.Features.Catalog.ListProducts;
using ShopSphere.Api.Features.Catalog.SearchProducts;
using ShopSphere.Domain.Catalog;
using ShopSphere.Domain.Common;
using ShopSphere.Infrastructure.Persistence;
using ShopSphere.UnitTests.Common;

namespace ShopSphere.UnitTests.Catalog;

public sealed class ProductHandlerTests
{
    [Fact]
    public async Task Delete_should_archive_existing_product()
    {
        await using var db = InMemoryDb.New();
        var product = await AddPublishedProduct(db, "Delete Me", "DELETE-1", 10m);

        await new DeleteProductHandler(db).Handle(
            new DeleteProductCommand(product.Id.Value), CancellationToken.None);

        (await db.Products.SingleAsync()).Status.Should().Be(ProductStatus.Archived);
    }

    [Fact]
    public async Task Delete_should_reject_missing_product()
    {
        await using var db = InMemoryDb.New();

        var act = () => new DeleteProductHandler(db).Handle(
            new DeleteProductCommand(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Update_should_apply_all_supplied_fields()
    {
        await using var db = InMemoryDb.New();
        var product = await AddPublishedProduct(db, "Old Name", "UPDATE-1", 10m);

        await new UpdateProductHandler(db).Handle(
            new UpdateProductCommand(product.Id.Value, "New Name", "New description", 25m, "eur"),
            CancellationToken.None);

        var saved = await db.Products.SingleAsync();
        saved.Title.Should().Be("New Name");
        saved.Slug.Value.Should().Be("new-name");
        saved.Description.Should().Be("New description");
        saved.Price.Should().Be(new Money(25m, "EUR"));
    }

    [Fact]
    public async Task Update_should_leave_fields_unchanged_when_optional_values_are_missing()
    {
        await using var db = InMemoryDb.New();
        var product = await AddPublishedProduct(db, "Original", "UPDATE-2", 10m);

        await new UpdateProductHandler(db).Handle(
            new UpdateProductCommand(product.Id.Value, null, null, null, null),
            CancellationToken.None);

        var saved = await db.Products.SingleAsync();
        saved.Title.Should().Be("Original");
        saved.Description.Should().Be("Description");
        saved.Price.Should().Be(new Money(10m, "USD"));
    }

    [Fact]
    public async Task Update_should_reject_missing_product()
    {
        await using var db = InMemoryDb.New();

        var act = () => new UpdateProductHandler(db).Handle(
            new UpdateProductCommand(Guid.NewGuid(), "Name", null, null, null), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task List_should_return_only_published_products_with_filters_and_sorting()
    {
        await using var db = InMemoryDb.New();
        var category = Category.Create("Catalog");
        var otherCategory = Category.Create("Other");
        db.Categories.AddRange(category, otherCategory);
        await db.SaveChangesAsync();
        await AddPublishedProduct(db, "Cheap", "LIST-1", 5m, category.Id);
        await AddPublishedProduct(db, "Expensive", "LIST-2", 25m, category.Id);
        await AddPublishedProduct(db, "Other", "LIST-3", 1m, otherCategory.Id);
        var draft = Product.Create("Draft", "Description", Sku.From("LIST-4"), category.Id, new Money(2m, "USD"));
        db.Products.Add(draft);
        await db.SaveChangesAsync();

        var result = await new ListProductsHandler(db).Handle(
            new ListProductsQuery(1, 1, category.Id.Value, 5m, 25m, "price_desc"), CancellationToken.None);

        result.TotalCount.Should().Be(2);
        result.Items.Should().ContainSingle();
        result.Items[0].Title.Should().Be("Expensive");
        result.HasNext.Should().BeTrue();
        result.HasPrevious.Should().BeFalse();
    }

    [Fact]
    public async Task List_should_sort_by_title_by_default_and_support_empty_results()
    {
        await using var db = InMemoryDb.New();
        var category = Category.Create("Catalog");
        db.Categories.Add(category);
        await db.SaveChangesAsync();
        await AddPublishedProduct(db, "Zebra", "LIST-5", 5m, category.Id);
        await AddPublishedProduct(db, "Alpha", "LIST-6", 5m, category.Id);

        var result = await new ListProductsHandler(db).Handle(
            new ListProductsQuery(1, 20), CancellationToken.None);
        var empty = await new ListProductsHandler(db).Handle(
            new ListProductsQuery(1, 20, MinPrice: 100m), CancellationToken.None);

        result.Items.Select(i => i.Title).Should().ContainInOrder("Alpha", "Zebra");
        empty.Items.Should().BeEmpty();
        empty.TotalPages.Should().Be(0);
    }

    [Fact]
    public async Task List_should_sort_by_price_ascending()
    {
        await using var db = InMemoryDb.New();
        var category = Category.Create("Catalog");
        db.Categories.Add(category);
        await db.SaveChangesAsync();
        await AddPublishedProduct(db, "Middle", "LIST-7", 10m, category.Id);
        await AddPublishedProduct(db, "Low", "LIST-8", 5m, category.Id);
        await AddPublishedProduct(db, "High", "LIST-9", 20m, category.Id);

        var result = await new ListProductsHandler(db).Handle(
            new ListProductsQuery(Sort: "price_asc"), CancellationToken.None);

        result.Items.Select(item => item.Title)
            .Should().ContainInOrder("Low", "Middle", "High");
    }

    [Fact]
    public async Task List_should_report_previous_page_when_pageing_results()
    {
        await using var db = InMemoryDb.New();
        var category = Category.Create("Catalog");
        db.Categories.Add(category);
        await db.SaveChangesAsync();
        await AddPublishedProduct(db, "Alpha", "LIST-10", 1m, category.Id);
        await AddPublishedProduct(db, "Beta", "LIST-11", 2m, category.Id);
        await AddPublishedProduct(db, "Gamma", "LIST-12", 3m, category.Id);

        var result = await new ListProductsHandler(db).Handle(
            new ListProductsQuery(Page: 2, PageSize: 2), CancellationToken.None);

        result.Items.Should().ContainSingle().Which.Title.Should().Be("Gamma");
        result.TotalPages.Should().Be(2);
        result.HasPrevious.Should().BeTrue();
        result.HasNext.Should().BeFalse();
    }

    [Fact]
    public async Task Search_should_match_title_or_description_and_exclude_drafts()
    {
        await using var db = InMemoryDb.New();
        var category = Category.Create("Catalog");
        db.Categories.Add(category);
        await db.SaveChangesAsync();
        await AddPublishedProduct(db, "Blue Chair", "SEARCH-1", 10m, category.Id, "Furniture");
        await AddPublishedProduct(db, "Desk", "SEARCH-2", 20m, category.Id, "Blue furniture");
        var draft = Product.Create("Blue Draft", "Draft", Sku.From("SEARCH-3"), category.Id, new Money(5m, "USD"));
        db.Products.Add(draft);
        await db.SaveChangesAsync();

        var result = await new SearchProductsHandler(db).Handle(
            new SearchProductsQuery("blue", 1, 20), CancellationToken.None);

        result.TotalCount.Should().Be(2);
        result.Items.Select(i => i.Title).Should().Contain("Blue Chair");
        result.Items.Select(i => i.Title).Should().Contain("Desk");
    }

    [Fact]
    public async Task Search_should_rank_title_matches_before_description_matches()
    {
        await using var db = InMemoryDb.New();
        var category = Category.Create("Catalog");
        db.Categories.Add(category);
        await db.SaveChangesAsync();
        await AddPublishedProduct(db, "A Desk", "SEARCH-4", 10m, category.Id, "Office");
        await AddPublishedProduct(db, "B Chair", "SEARCH-5", 10m, category.Id, "Desk furniture");

        var result = await new SearchProductsHandler(db).Handle(
            new SearchProductsQuery("desk"), CancellationToken.None);

        result.Items.Select(item => item.Title).Should().ContainInOrder("A Desk", "B Chair");
    }

    [Fact]
    public async Task Search_should_return_empty_page_when_term_has_no_matches()
    {
        await using var db = InMemoryDb.New();
        var category = Category.Create("Catalog");
        db.Categories.Add(category);
        await db.SaveChangesAsync();
        await AddPublishedProduct(db, "Desk", "SEARCH-6", 10m, category.Id);

        var result = await new SearchProductsHandler(db).Handle(
            new SearchProductsQuery("missing", Page: 2, PageSize: 5), CancellationToken.None);

        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.Page.Should().Be(2);
        result.HasPrevious.Should().BeTrue();
    }

    private static async Task<Product> AddPublishedProduct(
        ShopSphereDbContext db,
        string title,
        string sku,
        decimal price,
        CategoryId? categoryId = null,
        string description = "Description")
    {
        var category = categoryId ?? CategoryId.New();
        if (categoryId is null)
        {
            db.Categories.Add(Category.Create("Generated"));
            await db.SaveChangesAsync();
            category = (await db.Categories.LastAsync()).Id;
        }

        var product = Product.Create(title, description, Sku.From(sku), category, new Money(price, "USD"));
        product.Publish();
        db.Products.Add(product);
        await db.SaveChangesAsync();
        return product;
    }
}
