using ShopSphere.Domain.Common;

namespace ShopSphere.Domain.Users;

public sealed class Permission : AggregateRoot<PermissionId>
{
    private Permission() { }

    private Permission(
        PermissionId id,
        string name)
        : base(id)
    {
        Name = name;
    }

    public string Name { get; private set; } = default!;

    public static Permission Create(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new Permission(
            PermissionId.New(),
            name.Trim());
    }
}

public static class Permissions
{
    public const string ProductsWrite = "products.write";
    public const string ProductsRead = "products.read";

    public const string OrdersReadSelf = "orders.read.self";
    public const string OrdersReadAll = "orders.read.all";
    public const string OrdersManage = "orders.manage";

    public static readonly IReadOnlyList<string> All =
    [
        ProductsWrite,
        ProductsRead,
        OrdersReadSelf,
        OrdersReadAll,
        OrdersManage
    ];
}