using Microsoft.AspNetCore.WebUtilities;
using System.Security.Cryptography;
using System.Text;

namespace StudentApi.Presentation.Authentication;


/// Produces refresh tokens and hashes them with SHA-256 for safe storage.
public sealed class RefreshTokenService : IRefreshTokenService
{
    /// Generates a URL-safe 64-byte random refresh token.
    /// <returns>Opaque refresh token value sent to the client.</returns>
    public string GenerateToken()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(64);
        return WebEncoders.Base64UrlEncode(randomBytes);
    }

    /// Computes a SHA-256 hash of a refresh token for DB persistence and comparisons.
    /// <returns>Uppercase hexadecimal token hash.</returns>
    public string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes);
    }
}
