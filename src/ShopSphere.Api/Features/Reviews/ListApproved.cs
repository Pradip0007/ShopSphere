using ShopSphere.Domain.Catalog;
using ShopSphere.Domain.Reviews;

namespace ShopSphere.Api.Features.Reviews;

public static class ListApproved
{
    public sealed record Response(Guid ReviewId, int Rating, string Body, DateTimeOffset PostedAtUtc);

    public static async Task<IResult> HandleAsync(
        Guid productId,
        IReviewRepository reviews,
        CancellationToken ct)
    {
        var items = new List<Response>();
        await foreach (var r in reviews.ListApprovedForProductAsync(new ProductId(productId), ct))
        {
            items.Add(new Response(r.Id.Value, r.Rating, r.Body, r.PostedAtUtc));
        }
        return Results.Ok(items);
    }
}