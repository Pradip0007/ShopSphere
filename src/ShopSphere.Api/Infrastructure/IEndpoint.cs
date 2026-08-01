namespace ShopSphere.Api.Infrastructure;

/// <summary>
/// Marker interface implemented by every feature endpoint.
/// The DI container discovers all IEndpoint implementations and
/// invokes MapEndpoint on each during app startup.
/// </summary>
public interface IEndpoint
{
    void MapEndpoint(IEndpointRouteBuilder app);
}