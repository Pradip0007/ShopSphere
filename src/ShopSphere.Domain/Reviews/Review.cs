using ShopSphere.Domain.Catalog;
using ShopSphere.Domain.Common;

namespace ShopSphere.Domain.Reviews;

public sealed class Review : AggregateRoot<ReviewId>
{
    public Guid UserId { get; private set; }

    public ProductId ProductId { get; private set; }

    public int Rating { get; private set; }

    public string Body { get; private set; } = default!;

    public ReviewStatus Status { get; private set; }

    public DateTimeOffset PostedAtUtc { get; private set; }

    public DateTimeOffset? ModeratedAtUtc { get; private set; }

    public Guid? ModeratorUserId { get; private set; }

    public string? RejectionReason { get; private set; }

    // EF materialisation
    private Review() : base() { }

    private Review(
        ReviewId id,
        Guid userId,
        ProductId productId,
        int rating,
        string body)
        : base(id)
    {
        UserId = userId;
        ProductId = productId;
        Rating = rating;
        Body = body;
        Status = ReviewStatus.Pending;
        PostedAtUtc = DateTimeOffset.UtcNow;
    }

    public static Review Post(
        Guid userId,
        ProductId productId,
        int rating,
        string body)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(
            userId,
            Guid.Empty);

        if (rating < 1 || rating > 5)
        {
            throw new ArgumentOutOfRangeException(
                nameof(rating),
                "Rating must be 1..5.");
        }

        if (string.IsNullOrWhiteSpace(body))
        {
            throw new ArgumentException(
                "Body cannot be empty.",
                nameof(body));
        }

        if (body.Length > 4000)
        {
            throw new ArgumentException(
                "Body cannot exceed 4000 characters.",
                nameof(body));
        }

        var review = new Review(
            ReviewId.New(),
            userId,
            productId,
            rating,
            body.Trim());

        review.Raise(
            new ReviewPostedEvent(
                review.Id,
                userId,
                productId,
                rating,
                review.PostedAtUtc));

        return review;
    }

    public void Approve(Guid moderatorUserId)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(
            moderatorUserId,
            Guid.Empty);

        if (Status != ReviewStatus.Pending)
        {
            throw new InvalidOperationException(
                $"Cannot approve a review in state {Status}.");
        }

        Status = ReviewStatus.Approved;
        ModeratorUserId = moderatorUserId;
        ModeratedAtUtc = DateTimeOffset.UtcNow;
        RejectionReason = null;

        Raise(
            new ReviewApprovedEvent(
                Id,
                ProductId,
                moderatorUserId,
                ModeratedAtUtc.Value));
    }

    public void Reject(
        Guid moderatorUserId,
        string reason)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(
            moderatorUserId,
            Guid.Empty);

        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new ArgumentException(
                "Rejection reason required.",
                nameof(reason));
        }

        if (Status != ReviewStatus.Pending)
        {
            throw new InvalidOperationException(
                $"Cannot reject a review in state {Status}.");
        }

        Status = ReviewStatus.Rejected;
        ModeratorUserId = moderatorUserId;
        ModeratedAtUtc = DateTimeOffset.UtcNow;
        RejectionReason = reason.Trim();

        Raise(
            new ReviewRejectedEvent(
                Id,
                ProductId,
                moderatorUserId,
                RejectionReason,
                ModeratedAtUtc.Value));
    }
}