using System.Security.Cryptography;
using System.Text;

namespace StudentApi.Presentation.Authentication;

/// Password verifier for hashes generated using PBKDF2-SHA256.
/// Expected hash format: <c>{iterations}.{base64Salt}.{base64Hash}</c>.
public sealed class Pbkdf2PasswordHasher : IPasswordHasher
{
   
    /// Performs constant-time PBKDF2 hash comparison against stored credentials.
    /// <returns><c>true</c> when password matches the stored hash; otherwise <c>false</c>.</returns>
    public bool Verify(string password, string storedHash)
    {
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(storedHash))
        {
            return false;
        }

        var parts = storedHash.Split('.', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length != 3 || !int.TryParse(parts[0], out var iterations))
        {
            return false;
        }

        byte[] salt;
        byte[] expectedHash;

        try
        {
            salt = Convert.FromBase64String(parts[1]);
            expectedHash = Convert.FromBase64String(parts[2]);
        }
        catch (FormatException)
        {
            return false;
        }

        var calculatedHash = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            salt,
            iterations,
            HashAlgorithmName.SHA256,
            expectedHash.Length);

        return CryptographicOperations.FixedTimeEquals(calculatedHash, expectedHash);
    }
}
