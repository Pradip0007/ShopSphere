using ShopSphere.Api.Infrastructure;

namespace ShopSphere.Api.Features.Reviews;

public sealed class ReviewEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        // Public: list approved reviews for a product.
        app.MapGet("/products/{productId:guid}/reviews", ListApproved.HandleAsync)
            .WithTags("Reviews");

        // Authenticated user: post a review.
        app.MapPost("/products/{productId:guid}/reviews",
                async (Guid productId, PostReview.Request body, HttpContext http,
                       Domain.Reviews.IReviewRepository reviews, CancellationToken ct) =>
                    await PostReview.HandleAsync(
                        body with { ProductId = productId }, http, reviews, ct))
            .WithTags("Reviews")
            .RequireAuthorization();

        // Admin: approve / reject.
        var admin = app.MapGroup("/admin/reviews")
            .WithTags("Admin Reviews")
            .RequireAuthorization("admin");

        admin.MapPost("/{reviewId:guid}/approve", ShopSphere.Api.Features.Admin.ApproveReview.HandleAsync);
        admin.MapPost("/{reviewId:guid}/reject", ShopSphere.Api.Features.Admin.RejectReview.HandleAsync);
    }
}