using Microsoft.AspNetCore.WebUtilities;
using System.Security.Cryptography;
using System.Text;

namespace StudentApi.Presentation.Authentication;

public sealed class RefreshTokenService : IRefreshTokenService
{
    public string GenerateToken()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(64);
        return WebEncoders.Base64UrlEncode(randomBytes);
    }

    public string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes);
    }
}
