using Microsoft.AspNetCore.Http;
namespace ShopSphere.Api.Infrastructure;

public static class EndpointConventions
{
    /// <summary>
    /// Applies the feature tag and the standard ProblemDetails response docs
    /// that every endpoint in this API can produce (from the global handler).
    /// </summary>
    public static RouteHandlerBuilder WithFeature(this RouteHandlerBuilder builder, string tag)
        => builder
            .WithTags(tag)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
}