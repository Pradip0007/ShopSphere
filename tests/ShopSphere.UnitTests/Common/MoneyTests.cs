using ShopSphere.Domain.Common;

namespace ShopSphere.UnitTests.Common;

public sealed class MoneyTests
{
    [Fact]
    public void Constructor_should_normalize_currency()
    {
        var money = new Money(10.5m, " usd ");

        money.Currency.Should().Be("USD");
        money.Amount.Should().Be(10.5m);
    }

    [Fact]
    public void Constructor_should_reject_invalid_currency()
    {
        var act = () => new Money(10m, "US");

        act.Should()
            .Throw<ArgumentException>()
            .WithMessage("*3-letter ISO 4217 code*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("US1")]
    [InlineData("US D")]
    public void Constructor_should_reject_blank_or_non_letter_currency(string currency)
    {
        var act = () => new Money(10m, currency);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Zero_should_create_zero_money()
    {
        var money = Money.Zero("eur");

        money.Amount.Should().Be(0m);
        money.Currency.Should().Be("EUR");
    }

    [Fact]
    public void Addition_should_add_same_currency()
    {
        var left = new Money(10m, "USD");
        var right = new Money(5.5m, "USD");

        var result = left + right;

        result.Amount.Should().Be(15.5m);
        result.Currency.Should().Be("USD");
    }

    [Fact]
    public void Subtraction_should_subtract_same_currency()
    {
        var left = new Money(10m, "USD");
        var right = new Money(3.5m, "USD");

        var result = left - right;

        result.Amount.Should().Be(6.5m);
        result.Currency.Should().Be("USD");
    }

    [Fact]
    public void Multiplication_should_apply_factor()
    {
        var money = new Money(10m, "USD");

        var result = money * 2.5m;

        result.Amount.Should().Be(25m);
        result.Currency.Should().Be("USD");
    }

    [Fact]
    public void Reverse_multiplication_should_apply_factor()
    {
        var money = new Money(10m, "USD");

        var result = 2.5m * money;

        result.Amount.Should().Be(25m);
        result.Currency.Should().Be("USD");
    }

    [Fact]
    public void Arithmetic_should_reject_different_currencies()
    {
        var usd = new Money(10m, "USD");
        var eur = new Money(5m, "EUR");

        var act = () => usd + eur;

        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("*Currency mismatch*");
    }

    [Fact]
    public void Subtraction_should_reject_different_currencies()
    {
        var act = () => new Money(10m, "USD") - new Money(5m, "EUR");

        act.Should().Throw<InvalidOperationException>().WithMessage("*Currency mismatch*");
    }

    [Fact]
    public void Money_should_use_value_equality()
    {
        var left = new Money(10m, "usd");
        var right = new Money(10m, "USD");

        left.Should().Be(right);
    }

    [Fact]
    public void ToString_should_format_amount_and_currency()
    {
        var money = new Money(12.5m, "usd");

        money.ToString().Should().Be("12.50 USD");
    }
}