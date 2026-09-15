using ShopSphere.Domain.Cart;
using ShopSphere.Domain.Catalog;

namespace ShopSphere.UnitTests.Cart;

public sealed class CartTests
{
    [Fact]
    public void Empty_cart_should_report_is_empty()
    {
        var cart = new ShopSphere.Domain.Cart.Cart(
            CartKey.User(Guid.NewGuid()),
            []);

        cart.IsEmpty.Should().BeTrue();
        cart.TotalUnits.Should().Be(0);
    }

    [Fact]
    public void Cart_with_lines_should_not_be_empty()
    {
        var cart = new ShopSphere.Domain.Cart.Cart(
            CartKey.User(Guid.NewGuid()),
            [
                new CartLine(ProductId.New(), 2),
                new CartLine(ProductId.New(), 3)
            ]);

        cart.IsEmpty.Should().BeFalse();
        cart.TotalUnits.Should().Be(5);
    }

    [Fact]
    public void TotalUnits_should_sum_all_line_quantities()
    {
        var cart = new ShopSphere.Domain.Cart.Cart(
            CartKey.Session(Guid.NewGuid()),
            [
                new CartLine(ProductId.New(), 1),
                new CartLine(ProductId.New(), 4),
                new CartLine(ProductId.New(), 2)
            ]);

        cart.TotalUnits.Should().Be(7);
    }

    [Fact]
    public void User_key_should_create_user_redis_key()
    {
        var userId = Guid.NewGuid();

        var key = CartKey.User(userId);

        key.Kind.Should().Be(CartKeyKind.User);
        key.Value.Should().Be(userId);
        key.ToRedisKey().Should().Be($"cart:u:{userId:D}");
        key.ToString().Should().Be($"cart:u:{userId:D}");
    }

    [Fact]
    public void Session_key_should_create_session_redis_key()
    {
        var sessionId = Guid.NewGuid();

        var key = CartKey.Session(sessionId);

        key.Kind.Should().Be(CartKeyKind.Session);
        key.Value.Should().Be(sessionId);
        key.ToRedisKey().Should().Be($"cart:s:{sessionId:D}");
        key.ToString().Should().Be($"cart:s:{sessionId:D}");
    }

    [Fact]
    public void User_key_should_reject_empty_guid()
    {
        var act = () => CartKey.User(Guid.Empty);

        act.Should()
            .Throw<ArgumentException>()
            .WithMessage("*cannot be empty*");
    }

    [Fact]
    public void Session_key_should_reject_empty_guid()
    {
        var act = () => CartKey.Session(Guid.Empty);

        act.Should()
            .Throw<ArgumentException>()
            .WithMessage("*cannot be empty*");
    }
}