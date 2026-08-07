namespace ShopSphere.Domain.Common;

public readonly record struct PermissionId(Guid Value)
{
    public static PermissionId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}