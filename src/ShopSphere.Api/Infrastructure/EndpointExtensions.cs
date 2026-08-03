using System.Reflection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ShopSphere.Api.Infrastructure;

public static class EndpointExtensions
{
    /// <summary>
    /// Scans the supplied assembly for every non-abstract class implementing IEndpoint
    /// and registers it as a transient service.
    /// </summary>
    public static IServiceCollection AddEndpoints(this IServiceCollection services, Assembly assembly)
    {
        ServiceDescriptor[] descriptors = [.. assembly
            .DefinedTypes
            .Where(type => type is { IsAbstract: false, IsInterface: false }
                           && type.IsAssignableTo(typeof(IEndpoint)))
            .Select(type => ServiceDescriptor.Transient(typeof(IEndpoint), type))];

        services.TryAddEnumerable(descriptors);
        return services;
    }

    /// <summary>
    /// Resolves every registered IEndpoint and invokes MapEndpoint on it,
    /// optionally scoped under a RouteGroupBuilder (e.g. "/api/v1").
    /// </summary>
    public static IApplicationBuilder MapEndpoints(
    this WebApplication app,
    RouteGroupBuilder? routeGroupBuilder = null)
    {
        IEnumerable<IEndpoint> endpoints = app.Services.GetRequiredService<IEnumerable<IEndpoint>>();
        IEndpointRouteBuilder builder = routeGroupBuilder is null ? app : routeGroupBuilder;

        foreach (IEndpoint endpoint in endpoints)
        {
            endpoint.MapEndpoint(builder);
        }
        return app;
    }
}