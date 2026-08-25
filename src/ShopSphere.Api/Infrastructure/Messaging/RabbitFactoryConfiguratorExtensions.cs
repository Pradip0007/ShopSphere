using MassTransit;

namespace ShopSphere.Api.Infrastructure.Messaging;

public static class RabbitFactoryConfiguratorExtensions
{
    public static void UseShopSphereDefaults(
        this IRabbitMqBusFactoryConfigurator cfg)
    {
        cfg.UseMessageRetry(r => r.Exponential(
            retryLimit: 5,
            minInterval: TimeSpan.FromSeconds(1),
            maxInterval: TimeSpan.FromSeconds(30),
            intervalDelta: TimeSpan.FromSeconds(5)));

        cfg.UseKillSwitch(ks => ks
            .SetActivationThreshold(20)
            .SetTripThreshold(0.15)
            .SetRestartTimeout(TimeSpan.FromMinutes(1)));

        cfg.PrefetchCount = 16;
    }
}