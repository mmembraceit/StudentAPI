namespace StudentApi.Presentation.Authentication;


/// Generates and hashes refresh tokens used by the auth flow.
public interface IRefreshTokenService
{
    /// Creates a new cryptographically random refresh token value.
    /// <returns>Opaque token string that is returned to clients.</returns>
    string GenerateToken();

    /// Hashes a refresh token before persistence/lookup.
    /// <returns>Deterministic token hash representation.</returns>
    string HashToken(string token);
}
