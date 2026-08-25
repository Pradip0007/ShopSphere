using System.Security.Claims;
using ShopSphere.Domain.Catalog;
using ShopSphere.Domain.Reviews;

namespace ShopSphere.Api.Features.Reviews;

public static class PostReview
{
    public sealed record Request(Guid ProductId, int Rating, string Body);
    public sealed record Response(Guid ReviewId, string Status);

    public static async Task<IResult> HandleAsync(
        Request request,
        HttpContext http,
        IReviewRepository reviews,
        CancellationToken ct)
    {
        var userIdClaim = http.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? http.User.FindFirst("sub")?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Results.Unauthorized();
        }

        var productId = new ProductId(request.ProductId);

        if (await reviews.ExistsForUserAsync(userId, productId, ct))
        {
            return Results.Conflict(new { error = "You have already reviewed this product." });
        }

        Review review;
        try
        {
            review = Review.Post(userId, productId, request.Rating, request.Body);
        }
        catch (ArgumentException ex)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                [ex.ParamName ?? "input"] = [ex.Message]
            });
        }

        await reviews.AddAsync(review, ct);
        await reviews.SaveChangesAsync(ct);

        return Results.Created(
            $"/api/v1/reviews/{review.Id.Value:D}",
            new Response(review.Id.Value, review.Status.ToString()));
    }
}