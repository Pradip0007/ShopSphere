namespace ShopSphere.Domain.Cart;

/// <summary>
/// Identifies a cart. Either User(userId) or Session(sessionId).
/// Serializes to a Redis key of the form "cart:u:{guid}" or "cart:s:{guid}".
/// </summary>
public readonly record struct CartKey
{
    public CartKeyKind Kind { get; }
    public Guid Value { get; }

    private CartKey(CartKeyKind kind, Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("CartKey value cannot be empty.", nameof(value));
        }

        Kind = kind;
        Value = value;
    }

    public static CartKey User(Guid userId) => new(CartKeyKind.User, userId);
    public static CartKey Session(Guid sessionId) => new(CartKeyKind.Session, sessionId);

    public string ToRedisKey() => Kind switch
    {
        CartKeyKind.User    => $"cart:u:{Value:D}",
        CartKeyKind.Session => $"cart:s:{Value:D}",
        _                   => throw new InvalidOperationException($"Unknown CartKeyKind {Kind}")
    };

    public override string ToString() => ToRedisKey();
}

public enum CartKeyKind
{
    User,
    Session
}