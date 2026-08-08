using StackExchange.Redis;

namespace ShopSphere.Api.Infrastructure.Redis;

public static class RedisServiceCollectionExtensions
{
    public static IServiceCollection AddShopSphereRedis(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<RedisOptions>()
            .Bind(configuration.GetSection(RedisOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        var connectionString = configuration.GetConnectionString("cache")
            ?? throw new InvalidOperationException(
                "Missing connection string 'cache'. Did AppHost forget .WithReference(cache)?");

        services.AddSingleton<IConnectionMultiplexer>(_ =>
        {
            var options = ConfigurationOptions.Parse(connectionString);
            options.AbortOnConnectFail = false; // let the app start even if Redis is briefly down
            options.ClientName = "ShopSphere.Api";
            return ConnectionMultiplexer.Connect(options);
        });

        // Convenience: expose the default database as a scoped service.
        services.AddScoped(sp => sp.GetRequiredService<IConnectionMultiplexer>().GetDatabase());

        return services;
    }
}