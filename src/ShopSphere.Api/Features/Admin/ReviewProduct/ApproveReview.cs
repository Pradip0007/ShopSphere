using System.Security.Claims;
using ShopSphere.Domain.Reviews;

namespace ShopSphere.Api.Features.Admin;

public static class ApproveReview
{
    public static async Task<IResult> HandleAsync(
        Guid reviewId,
        HttpContext http,
        IReviewRepository reviews,
        CancellationToken ct)
    {
        var moderatorClaim = http.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(moderatorClaim, out var moderator))
        {
            return Results.Unauthorized();
        }

        var review = await reviews.FindAsync(new ReviewId(reviewId), ct);
        if (review is null) return Results.NotFound();

        try
        {
            review.Approve(moderator);
        }
        catch (InvalidOperationException ex)
        {
            return Results.Conflict(new { error = ex.Message });
        }

        await reviews.SaveChangesAsync(ct);
        return Results.NoContent();
    }
}