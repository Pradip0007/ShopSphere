namespace ShopSphere.Api.Infrastructure;

public static class EndpointConventions
{
    /// <summary>
    /// Common conventions applied to every feature endpoint.
    /// Extend on Day 20 with OpenAPI metadata; on Day 21 with versioning.
    /// </summary>
    public static RouteHandlerBuilder WithFeature(this RouteHandlerBuilder builder, string tag)
        => builder.WithTags(tag);
}