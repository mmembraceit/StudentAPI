namespace StudentApi.Presentation.Authentication;


/// Creates signed JWT access tokens for authenticated users.

public interface IJwtTokenService
{
    /// Generates a JWT access token containing identity and role claims.
    /// <returns>Serialized JWT token string.</returns>
    string GenerateToken(string username, string role);
}
