using System.Reflection;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using ShopSphere.Api.Infrastructure.Inventory;
using ShopSphere.Api.Infrastructure.Outbox;
using ShopSphere.Workers.Jobs;
using StackExchange.Redis;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

var redisConn =
    builder.Configuration.GetConnectionString("cache")
    ?? throw new InvalidOperationException(
        "Missing connection string 'cache'.");

builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
{
    var options = ConfigurationOptions.Parse(redisConn);

    options.AbortOnConnectFail = false;
    options.ClientName = "ShopSphere.Workers";

    return ConnectionMultiplexer.Connect(options);
});

var rabbitConn =
    builder.Configuration.GetConnectionString("rabbit")
    ?? throw new InvalidOperationException(
        "Missing connection string 'rabbit'.");

builder.Services.AddMassTransit(x =>
{
    x.SetKebabCaseEndpointNameFormatter();

    x.AddConsumers(
        Assembly.GetExecutingAssembly());

    x.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host(new Uri(rabbitConn));
        cfg.ConfigureEndpoints(ctx);
    });
});

builder.Services.AddDbContext<OutboxDbContext>(o =>
    o.UseSqlite(builder.Configuration.GetConnectionString("outbox")
        ?? "Data Source=../shopsphere-outbox.db"));

builder.Services.AddSingleton<IStockService, InMemoryStockService>();

builder.Services.AddHostedService<AbandonedCartReminderJob>();
builder.Services.AddHostedService<InventorySnapshotJob>();
builder.Services.AddHostedService<OutboxDispatcherJob>();

var host = builder.Build();

// Seed for demo
var stock = host.Services.GetRequiredService<IStockService>();
for (var i = 0; i < 5; i++)
{
    _ = await stock.ReserveAsync(Guid.NewGuid(), 0);
}

host.Run();