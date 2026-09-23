namespace ShopSphere.Domain.Users;

public readonly record struct VerificationTokenId(Guid Value)
{
    public static VerificationTokenId New() => new(Guid.NewGuid());
}