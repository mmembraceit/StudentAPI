namespace StudentApi.Presentation.Authentication;

/// <summary>
/// Generates and hashes refresh tokens used by the auth flow.
/// </summary>
public interface IRefreshTokenService
{
    /// <summary>
    /// Creates a new cryptographically random refresh token value.
    /// </summary>
    /// <returns>Opaque token string that is returned to clients.</returns>
    string GenerateToken();

    /// <summary>
    /// Hashes a refresh token before persistence/lookup.
    /// </summary>
    /// <param name="token">Raw refresh token.</param>
    /// <returns>Deterministic token hash representation.</returns>
    string HashToken(string token);
}
