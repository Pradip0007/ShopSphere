namespace ShopSphere.Domain.Reviews;

public readonly record struct ReviewId(Guid Value)
{
    public static ReviewId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString("D");
}