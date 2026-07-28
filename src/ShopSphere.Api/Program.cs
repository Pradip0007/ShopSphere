
using ShopSphere.Domain.Catalog;
using ShopSphere.Domain.Common;
using ShopSphere.Domain.Inventory;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

var app = builder.Build();

app.MapDefaultEndpoints();

app.MapGet("/", () => "ShopSphere API — Day 7 alive!");

app.MapGet("/_debug/stock", () =>
{
    var stock = StockLevel.Create(ProductId.New(), initialAvailable: 3);

    var r1 = stock.Reserve(2);
    var r2 = stock.Reserve(1);   // drives Available to 0 — depletion event
    var r3 = stock.Reserve(1);   // insufficient stock — Failure
    var r4 = stock.Release(1);
    var r5 = stock.Adjust(-99);  // over-drain — Failure

    return new
    {
        stock.Available,
        stock.Reserved,
        results = new[]
        {
            new { call = "Reserve(2)", r1.IsSuccess, r1.Error },
            new { call = "Reserve(1)", r2.IsSuccess, r2.Error },
            new { call = "Reserve(1)", r3.IsSuccess, r3.Error },
            new { call = "Release(1)", r4.IsSuccess, r4.Error },
            new { call = "Adjust(-99)", r5.IsSuccess, r5.Error }
        },
        events = stock.DomainEvents.Select(e => e.GetType().Name).ToArray()
    };
});

app.Run();