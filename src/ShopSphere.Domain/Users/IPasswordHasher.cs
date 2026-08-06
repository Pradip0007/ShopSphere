namespace ShopSphere.Domain.Users;

public interface IPasswordHasher
{
    /// <summary>
    /// Returns a self-describing hash string (algorithm + params + salt + hash).
    /// Never returns the plaintext.
    /// </summary>
    string Hash(string plainPassword);

    /// <summary>
    /// Verifies plaintext against a previously-stored hash. Timing-safe.
    /// </summary>
    bool Verify(string plainPassword, string storedHash);
}