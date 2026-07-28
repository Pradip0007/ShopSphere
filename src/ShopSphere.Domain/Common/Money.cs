namespace ShopSphere.Domain.Common;

/// <summary>
/// TEMPORARY stub. Day 8 replaces this with a full value object
/// (arithmetic operators, currency guards, ISO validation, Zero factory).
/// </summary>
public sealed record Money(decimal Amount, string Currency)
{
    public override string ToString() => $"{Amount:0.00} {Currency}";
}