
using ShopSphere.Domain.Catalog;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

var app = builder.Build();

app.MapDefaultEndpoints();

app.MapGet("/", () => "ShopSphere API — Day 5 alive!");

app.MapGet("/_debug/category", (string name) =>
{
    var category = Category.Create(name);
    return new
    {
        id = category.Id.ToString(),
        category.Name,
        slug = category.Slug.Value,
        parentId = category.ParentId?.ToString(),
        events = category.DomainEvents.Select(e => e.GetType().Name).ToArray()
    };
});


app.Run();