using System.Security.Claims;
using ShopSphere.Domain.Reviews;

namespace ShopSphere.Api.Features.Admin;

public static class RejectReview
{
    public sealed record Request(string Reason);

    public static async Task<IResult> HandleAsync(
        Guid reviewId,
        Request request,
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
            review.Reject(moderator, request.Reason);
        }
        catch (ArgumentException ex)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                [ex.ParamName ?? "reason"] = [ex.Message]
            });
        }
        catch (InvalidOperationException ex)
        {
            return Results.Conflict(new { error = ex.Message });
        }

        await reviews.SaveChangesAsync(ct);
        return Results.NoContent();
    }
}