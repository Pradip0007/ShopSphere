using ShopSphere.Domain.Catalog;
using ShopSphere.Domain.Reviews;

namespace ShopSphere.UnitTests.Reviews;

public sealed class ReviewTests
{
    private static Review NewReview(
        int rating = 5,
        string body = "Great product") =>
        Review.Post(
            userId: Guid.NewGuid(),
            productId: ProductId.New(),
            rating: rating,
            body: body);

    [Fact]
    public void Post_should_create_pending_review()
    {
        var review = NewReview();

        review.Status.Should().Be(ReviewStatus.Pending);
        review.Rating.Should().Be(5);
        review.Body.Should().Be("Great product");
    }

    [Fact]
    public void Post_should_trim_body()
    {
        var review = NewReview(body: "  Great product  ");

        review.Body.Should().Be("Great product");
    }

    [Fact]
    public void Post_should_raise_posted_event()
    {
        var review = NewReview();

        review.DomainEvents.Should()
            .ContainSingle(e => e is ReviewPostedEvent);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    public void Post_should_reject_rating_outside_1_to_5(int rating)
    {
        var act = () => NewReview(rating: rating);

        act.Should()
            .Throw<ArgumentOutOfRangeException>()
            .WithMessage("*Rating must be 1..5*");
    }

    [Fact]
    public void Post_should_reject_empty_body()
    {
        var act = () => NewReview(body: "   ");

        act.Should()
            .Throw<ArgumentException>()
            .WithMessage("*Body cannot be empty*");
    }

    [Fact]
    public void Post_should_reject_body_longer_than_4000_characters()
    {
        var act = () => NewReview(body: new string('x', 4001));

        act.Should()
            .Throw<ArgumentException>()
            .WithMessage("*4000 characters*");
    }

    [Fact]
    public void Approve_should_set_approved_state_and_moderator()
    {
        var review = NewReview();
        var moderatorId = Guid.NewGuid();

        review.Approve(moderatorId);

        review.Status.Should().Be(ReviewStatus.Approved);
        review.ModeratorUserId.Should().Be(moderatorId);
        review.ModeratedAtUtc.Should().NotBeNull();
        review.RejectionReason.Should().BeNull();
    }

    [Fact]
    public void Approve_should_raise_approved_event()
    {
        var review = NewReview();

        review.Approve(Guid.NewGuid());

        review.DomainEvents.Should()
            .ContainSingle(e => e is ReviewApprovedEvent);
    }

    [Fact]
    public void Approve_should_reject_already_moderated_review()
    {
        var review = NewReview();
        review.Approve(Guid.NewGuid());

        var act = () => review.Approve(Guid.NewGuid());

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Reject_should_set_rejected_state_and_reason()
    {
        var review = NewReview();
        var moderatorId = Guid.NewGuid();

        review.Reject(moderatorId, "  Not helpful  ");

        review.Status.Should().Be(ReviewStatus.Rejected);
        review.ModeratorUserId.Should().Be(moderatorId);
        review.ModeratedAtUtc.Should().NotBeNull();
        review.RejectionReason.Should().Be("Not helpful");
    }

    [Fact]
    public void Reject_should_raise_rejected_event()
    {
        var review = NewReview();

        review.Reject(Guid.NewGuid(), "Not helpful");

        review.DomainEvents.Should()
            .ContainSingle(e => e is ReviewRejectedEvent);
    }

    [Fact]
    public void Reject_should_reject_empty_reason()
    {
        var review = NewReview();

        var act = () => review.Reject(Guid.NewGuid(), "   ");

        act.Should()
            .Throw<ArgumentException>()
            .WithMessage("*Rejection reason required*");
    }

    [Fact]
    public void Reject_should_reject_already_moderated_review()
    {
        var review = NewReview();
        review.Reject(Guid.NewGuid(), "Not helpful");

        var act = () => review.Reject(Guid.NewGuid(), "Another reason");

        act.Should().Throw<InvalidOperationException>();
    }
}