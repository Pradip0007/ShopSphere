using ShopSphere.Domain.Cart;

namespace ShopSphere.Api.Infrastructure.Cart;

public static class CartServiceCollectionExtensions
{
    public static IServiceCollection AddShopSphereCart(this IServiceCollection services)
    {
        services.AddScoped<ICartRepository, RedisCartRepository>();
        return services;
    }
}