using System.Reflection;
using MassTransit;

namespace ShopSphere.Api.Infrastructure.Messaging;

public static class MassTransitServiceCollectionExtensions
{
    public static IServiceCollection AddShopSphereMessaging(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("rabbit")
            ?? throw new InvalidOperationException(
                "Missing connection string 'rabbit'. Did AppHost forget .WithReference(rabbit)?");

        services.AddMassTransit(x =>
        {
            // Prefix queue names so a shared Rabbit dev instance stays tidy.
            x.SetKebabCaseEndpointNameFormatter();

            // Pick up every IConsumer<T> in this assembly.
            x.AddConsumers(Assembly.GetExecutingAssembly());

            x.UsingRabbitMq((ctx, cfg) =>
            {
                cfg.Host(new Uri(connectionString));
                cfg.ConfigureEndpoints(ctx);

                // Reasonable defaults; Day 47 tunes these per consumer.
                cfg.UseMessageRetry(r => r.Immediate(3));
                cfg.PrefetchCount = 16;
            });
        });

        return services;
    }
}