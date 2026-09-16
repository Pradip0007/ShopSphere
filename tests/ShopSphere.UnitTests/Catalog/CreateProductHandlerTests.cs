using ShopSphere.Api.Features.Admin.CreateProduct;
using ShopSphere.Domain.Catalog;
using ShopSphere.Domain.Inventory;
using ShopSphere.Infrastructure.Persistence;
using ShopSphere.UnitTests.Common;
using ShopSphere.Api.Middleware;
using ShopSphere.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace ShopSphere.UnitTests.Catalog;

public sealed class CreateProductHandlerTests
{
    [Fact]
    public async Task Handle_should_persist_published_product_and_stock()
    {
        await using var db = InMemoryDb.New();

        var category = Category.Create("Electronics");
        db.Categories.Add(category);
        await db.SaveChangesAsync();

        var handler = new CreateProductHandler(db);

        var command = new CreateProductCommand(
            Title: "Widget",
            Sku: "SKU-42",
            Description: "Test widget",
            Price: 19.99m,
            Currency: "USD",
            InitialStock: 25,
            CategoryId: category.Id.Value);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Id.Should().NotBe(Guid.Empty);

        var savedProduct = (await db.Products
        .Where(p => p.Id.Value == result.Id)
        .ToListAsync())
        .SingleOrDefault();
        savedProduct.Should().NotBeNull();
        savedProduct!.Title.Should().Be("Widget");
        savedProduct.Sku.Value.Should().Be("SKU-42");
        savedProduct.Status.Should().Be(ProductStatus.Published);
        savedProduct.CategoryId.Should().Be(category.Id);

        var savedStock = (await db.StockLevels
        .Where(s => s.ProductId.Value == result.Id)
        .ToListAsync())
        .SingleOrDefault();
        savedStock.Should().NotBeNull();
        savedStock!.ProductId.Should().Be(savedProduct.Id);
        savedStock.Sku.Should().Be(savedProduct.Sku);
        savedStock.Available.Should().Be(25);
        savedStock.Reserved.Should().Be(0);
    }

    [Fact]
    public async Task Handle_should_reject_missing_category()
    {
        await using var db = InMemoryDb.New();

        var handler = new CreateProductHandler(db);

        var command = new CreateProductCommand(
            "Widget",
            "SKU-1",
            "Test widget",
            10m,
            "USD",
            5,
            Guid.NewGuid());

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should()
            .ThrowAsync<KeyNotFoundException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public async Task Handle_should_reject_duplicate_sku()
    {
        await using var db = InMemoryDb.New();

        var category = Category.Create("Electronics");
        db.Categories.Add(category);

        var existingProduct = Product.Create(
            title: "Existing",
            description: "Existing product",
            sku: Sku.From("SKU-1"),
            categoryId: category.Id,
            price: new Money(10m, "USD"));

        db.Products.Add(existingProduct);
        await db.SaveChangesAsync();

        var handler = new CreateProductHandler(db);

        var command = new CreateProductCommand(
            "Duplicate",
            "SKU-1",
            "Duplicate product",
            20m,
            "USD",
            5,
            category.Id.Value);

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should()
            .ThrowAsync<ConflictException>()
            .WithMessage("*already in use*");
    }

    [Fact]
    public async Task Handle_should_create_stock_with_zero_initial_stock()
    {
        await using var db = InMemoryDb.New();

        var category = Category.Create("Electronics");
        db.Categories.Add(category);
        await db.SaveChangesAsync();

        var handler = new CreateProductHandler(db);

        var command = new CreateProductCommand(
            "Widget",
            "SKU-ZERO",
            "Zero stock widget",
            15m,
            "USD",
            0,
            category.Id.Value);

        var result = await handler.Handle(command, CancellationToken.None);

       var stock = (await db.StockLevels
        .Where(s => s.ProductId.Value == result.Id)
        .ToListAsync())
        .SingleOrDefault();

        stock.Should().NotBeNull();
        stock!.Available.Should().Be(0);
        stock.Reserved.Should().Be(0);
    }
}