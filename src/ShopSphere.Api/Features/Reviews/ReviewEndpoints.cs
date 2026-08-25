namespace ShopSphere.Api.Features.Reviews;

public static class ReviewEndpoints
{
    public static IEndpointRouteBuilder MapReviewEndpoints(this IEndpointRouteBuilder routes)
    {
        // Public: list approved reviews for a product.
        routes.MapGet("/api/v1/products/{productId:guid}/reviews", ListApproved.HandleAsync)
            .WithTags("Reviews");

        // Authenticated user: post a review.
        routes.MapPost("/api/v1/products/{productId:guid}/reviews",
                async (Guid productId, PostReview.Request body, HttpContext http,
                       Domain.Reviews.IReviewRepository reviews, CancellationToken ct) =>
                    await PostReview.HandleAsync(
                        body with { ProductId = productId }, http, reviews, ct))
            .WithTags("Reviews")
            .RequireAuthorization();

        // Admin: approve / reject.
        var admin = routes.MapGroup("/api/v1/admin/reviews")
            .WithTags("Admin Reviews")
            .RequireAuthorization("admin");

        admin.MapPost("/{reviewId:guid}/approve", ShopSphere.Api.Features.Admin.ApproveReview.HandleAsync);
        admin.MapPost("/{reviewId:guid}/reject", ShopSphere.Api.Features.Admin.RejectReview.HandleAsync);

        return routes;
    }
}