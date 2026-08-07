using ShopSphere.Domain.Common;

namespace ShopSphere.Domain.Users;

public sealed class Role : AggregateRoot<RoleId>
{
    private readonly List<Permission> _permissions = [];

    private Role() { }

    private Role(RoleId id, string name) : base(id)
    {
        Name = name;
    }

    public string Name { get; private set; } = default!;

    public IReadOnlyCollection<Permission> Permissions =>
        _permissions.AsReadOnly();

    public static Role Create(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new Role(
            RoleId.New(),
            name.Trim());
    }

    public void Grant(Permission permission)
    {
        ArgumentNullException.ThrowIfNull(permission);

        if (_permissions.Any(p => p.Id == permission.Id))
            return;

        _permissions.Add(permission);
    }

    public void Revoke(Permission permission)
    {
        ArgumentNullException.ThrowIfNull(permission);

        Permission? existing =
            _permissions.FirstOrDefault(p => p.Id == permission.Id);

        if (existing is not null)
        {
            _permissions.Remove(existing);
        }
    }
}