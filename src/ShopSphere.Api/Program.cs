using ShopSphere.Domain.Catalog;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

var app = builder.Build();

app.MapDefaultEndpoints();

app.MapGet("/", () => "ShopSphere API — Day 4 alive!");

app.MapGet("/_debug/raise", (string? msg) =>
{
    var agg = PingAggregate.Create(msg ?? "hello");
    var events = agg.DomainEvents.Select(e => new
    {
        type = e.GetType().Name,
        e.EventId,
        e.OccurredAt
    }).ToArray();

    agg.ClearDomainEvents();

    return new
    {
        aggregateId = agg.Id,
        raised = events,
        afterClear = agg.DomainEvents.Count
    };
});

app.Run();