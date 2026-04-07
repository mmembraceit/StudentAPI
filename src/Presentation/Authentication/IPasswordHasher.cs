namespace StudentApi.Presentation.Authentication;

/// <summary>
/// Verifies user-provided passwords against stored password hashes.
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Validates a plain password against the persisted hash format.
    /// </summary>
    /// <param name="password">Raw password from login request.</param>
    /// <param name="storedHash">Stored hash from persistence.</param>
    /// <returns><c>true</c> when password matches; otherwise <c>false</c>.</returns>
    bool Verify(string password, string storedHash);
}
