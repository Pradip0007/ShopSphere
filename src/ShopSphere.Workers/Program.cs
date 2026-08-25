using System.Reflection;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using ShopSphere.Infrastructure.Inventory;
using ShopSphere.Infrastructure.Outbox;
using ShopSphere.Workers.Jobs;
using StackExchange.Redis;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

// ------------------------------------------------------------
// Redis
// ------------------------------------------------------------

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

// ------------------------------------------------------------
// RabbitMQ
// ------------------------------------------------------------

var rabbitConn =
    builder.Configuration.GetConnectionString("rabbit")
    ?? throw new InvalidOperationException(
        "Missing connection string 'rabbit'.");


// Temporary local credentials for Day 47 DLQ monitoring.
// DO NOT COMMIT THESE CREDENTIALS.
var rabbitUser = "guest";
var rabbitPassword = "ub13tez7BUpm9p1S41DsMR";

builder.Services.AddMassTransit(x =>
{
    x.SetKebabCaseEndpointNameFormatter();

    x.AddConsumers(
        Assembly.GetExecutingAssembly());

    x.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host(new Uri(rabbitConn));

        // Day 47 demo:
        // 1 initial attempt + 3 immediate retries
        // = 4 total attempts.
        //
        // Restore the exponential policy after testing.
        cfg.UseMessageRetry(r => r.Exponential(
            retryLimit: 5,
            minInterval: TimeSpan.FromSeconds(1),
            maxInterval: TimeSpan.FromSeconds(30),
            intervalDelta: TimeSpan.FromSeconds(5)));
        // Stop the endpoint if too many messages
        // are failing.
        cfg.UseKillSwitch(ks => ks
            .SetActivationThreshold(20)
            .SetTripThreshold(0.15)
            .SetRestartTimeout(
                TimeSpan.FromMinutes(1)));

        // Maximum number of messages held in-flight.
        cfg.PrefetchCount = 16;

        // Automatically configure consumer endpoints.
        cfg.ConfigureEndpoints(ctx);
    });
});

// ------------------------------------------------------------
// Outbox
// ------------------------------------------------------------

builder.Services.AddDbContext<OutboxDbContext>(o =>
    o.UseSqlite(
        builder.Configuration.GetConnectionString("outbox")
        ?? "Data Source=../shopsphere-outbox.db"));

// ------------------------------------------------------------
// Inventory
// ------------------------------------------------------------

builder.Services.AddSingleton<IStockService, InMemoryStockService>();

// ------------------------------------------------------------
// Background jobs
// ------------------------------------------------------------

builder.Services.AddHostedService<AbandonedCartReminderJob>();

builder.Services.AddHostedService<InventorySnapshotJob>();

builder.Services.AddHostedService<OutboxDispatcherJob>();

// ------------------------------------------------------------
// RabbitMQ Dead-Letter Queue monitor
// ------------------------------------------------------------

builder.Services.AddHttpClient("RabbitManagement", client =>
{
    client.BaseAddress =
        new Uri("http://localhost:15672/");

    var basic = Convert.ToBase64String(
        System.Text.Encoding.ASCII.GetBytes(
            $"{rabbitUser}:{rabbitPassword}"));

    client.DefaultRequestHeaders.Authorization =
        new System.Net.Http.Headers.AuthenticationHeaderValue(
            "Basic",
            basic);
});

builder.Services.AddHostedService<DeadLetterMonitorJob>();

// ------------------------------------------------------------
// Build host
// ------------------------------------------------------------

var host = builder.Build();

// ------------------------------------------------------------
// Demo inventory seed
// ------------------------------------------------------------

var stock =
    host.Services.GetRequiredService<IStockService>();

for (var i = 0; i < 5; i++)
{
    _ = await stock.ReserveAsync(
        Guid.NewGuid(),
        0);
}

// ------------------------------------------------------------
// Start Workers
// ------------------------------------------------------------

host.Run();