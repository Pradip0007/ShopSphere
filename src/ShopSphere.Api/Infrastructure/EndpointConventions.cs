using Microsoft.AspNetCore.Http;

namespace ShopSphere.Api.Infrastructure;

public static class EndpointConventions
{
    public static RouteHandlerBuilder WithFeature(this RouteHandlerBuilder builder, string tag)
        => builder
            .WithTags(tag)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
}