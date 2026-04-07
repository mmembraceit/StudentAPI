namespace StudentApi.Presentation.Authentication;

/// <summary>
/// Creates signed JWT access tokens for authenticated users.
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    /// Generates a JWT access token containing identity and role claims.
    /// </summary>
    /// <param name="username">Authenticated username embedded in token claims.</param>
    /// <param name="role">Role claim used by authorization policies.</param>
    /// <returns>Serialized JWT token string.</returns>
    string GenerateToken(string username, string role);
}
