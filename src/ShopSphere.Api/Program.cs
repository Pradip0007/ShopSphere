
using ShopSphere.Domain.Catalog;
using ShopSphere.Domain.Common;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

var app = builder.Build();

app.MapDefaultEndpoints();

app.MapGet("/", () => "ShopSphere API — Day 6 alive!");

app.MapGet("/_debug/category", (string name) =>
{
    var category = Category.Create(name);
    return new
    {
        id = category.Id.ToString(),
        category.Name,
        slug = category.Slug.Value
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


app.Run();