using System.Security.Cryptography;
using System.Text;
using Konscious.Security.Cryptography;
using ShopSphere.Domain.Users;

namespace ShopSphere.Infrastructure.Security;

public sealed class Argon2PasswordHasher : IPasswordHasher
{
    // OWASP baseline for Argon2id (2024). Adjust after benchmarking on prod hardware.
    private const int MemoryKiB = 64 * 1024;   // 64 MiB
    private const int Iterations = 3;
    private const int DegreeOfParallelism = 1;
    private const int SaltBytes = 16;
    private const int HashBytes = 32;

    public string Hash(string plainPassword)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(SaltBytes);
        byte[] hash = ComputeHash(plainPassword, salt);

        // Format: $argon2id$v=19$m=<m>,t=<t>,p=<p>$<salt-b64>$<hash-b64>
        return string.Create(
            System.Globalization.CultureInfo.InvariantCulture,
            $"$argon2id$v=19$m={MemoryKiB},t={Iterations},p={DegreeOfParallelism}$" +
            $"{Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}");
    }

    public bool Verify(string plainPassword, string storedHash)
    {
        try
        {
            string[] parts = storedHash.Split('$', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 5) return false;

            byte[] salt = Convert.FromBase64String(parts[3]);
            byte[] expected = Convert.FromBase64String(parts[4]);
            byte[] actual = ComputeHash(plainPassword, salt);

            return CryptographicOperations.FixedTimeEquals(expected, actual);
        }
        catch (FormatException)
        {
            return false;
        }
    }

    private static byte[] ComputeHash(string plainPassword, byte[] salt)
    {
        using Argon2id argon = new(Encoding.UTF8.GetBytes(plainPassword))
        {
            Salt = salt,
            MemorySize = MemoryKiB,
            Iterations = Iterations,
            DegreeOfParallelism = DegreeOfParallelism,
        };
        return argon.GetBytes(HashBytes);
    }
}