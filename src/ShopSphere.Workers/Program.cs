using System.Reflection;
using MassTransit;
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

builder.Services.AddHostedService<
    AbandonedCartReminderJob>();

var host = builder.Build();

host.Run();