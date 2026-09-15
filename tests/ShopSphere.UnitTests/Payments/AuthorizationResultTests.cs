using ShopSphere.Domain.Payments;

namespace ShopSphere.UnitTests.Payments;

public sealed class AuthorizationResultTests
{
    [Fact]
    public void AuthorizationResult_should_capture_success_details()
    {
        var result = new AuthorizationResult(true, "pi_123", null);

        result.Succeeded.Should().BeTrue();
        result.PaymentIntentId.Should().Be("pi_123");
        result.DeclineReason.Should().BeNull();
    }

    [Fact]
    public void AuthorizationResult_should_capture_decline_details_and_use_value_equality()
    {
        var result = new AuthorizationResult(false, null, "Card declined");

        result.Succeeded.Should().BeFalse();
        result.PaymentIntentId.Should().BeNull();
        result.DeclineReason.Should().Be("Card declined");
        result.Should().Be(new AuthorizationResult(false, null, "Card declined"));
    }
}
