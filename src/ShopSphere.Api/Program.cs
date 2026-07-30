
using ShopSphere.Domain.Catalog;
using ShopSphere.Domain.Common;
using ShopSphere.Domain.Inventory;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

var app = builder.Build();

app.MapDefaultEndpoints();

app.MapGet("/", () => "ShopSphere API — Day 9 alive!");

app.MapGet("/_debug/money", () =>
{
    var ten = new Money(10m, "gbp");   // normalises to GBP
    var five = new Money(5m, "GBP");
    var sum = ten + five;
    var diff = ten - five;
    var scaled = ten * 1.2m;           // 20% VAT

    return new
    {
        ten = ten.ToString(),
        five = five.ToString(),
        sum = sum.ToString(),
        diff = diff.ToString(),
        vatInclusive = scaled.ToString(),
        zero = Money.Zero("USD").ToString()
    };
});

app.MapGet("/_debug/product", (string title) =>
{
    var category = Category.Create("Demo Category");
    var product = Product.Create(
        title: title,
        description: "Placeholder description.",
        sku: Sku.From("DEMO-001"),
        categoryId: category.Id,
        price: new Money(19.99m, "GBP"));

    product.Publish();

    return new
    {
        id = product.Id.ToString(),
        product.Title,
        slug = product.Slug.Value,
        sku = product.Sku.Value,
        price = product.Price.ToString(),
        status = product.Status.ToString(),
        events = product.DomainEvents.Select(e => e.GetType().Name).ToArray()
    };
});

app.MapGet("/_debug/stock", () =>
{
    var stock = StockLevel.Create(ProductId.New(), initialAvailable: 3);

    var r1 = stock.Reserve(2);
    var r2 = stock.Reserve(1);
    var r3 = stock.Reserve(1);   // insufficient
    var r4 = stock.Release(1);
    var r5 = stock.Adjust(-99);  // below zero

    static object Render(string call, Result r) => new
    {
        call,
        r.IsSuccess,
        error = r.IsSuccess ? null : new { r.Error.Code, r.Error.Message }
    };

    return new
    {
        stock.Available,
        stock.Reserved,
        results = new[]
        {
            Render("Reserve(2)", r1),
            Render("Reserve(1)", r2),
            Render("Reserve(1)", r3),
            Render("Release(1)", r4),
            Render("Adjust(-99)", r5)
        },
        events = stock.DomainEvents.Select(e => e.GetType().Name).ToArray()
    };
});

app.Run();