namespace StudentApi.Presentation.Authentication;


/// Verifies user-provided passwords against stored password hashes.

public interface IPasswordHasher
{
    /// Validates a plain password against the persisted hash format.
    /// <returns><c>true</c> when password matches; otherwise <c>false</c>.</returns>
    bool Verify(string password, string storedHash);
}
