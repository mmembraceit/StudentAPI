namespace StudentApi.Domain.Entities;

/// <summary>
/// Domain entity that represents an authenticated API user account.
/// - User account identifier.
/// - Unique username used for authentication.
/// - Persisted password hash.
/// - Authorization role for policy checks.
/// - Indicates whether the account can authenticate.
/// </summary>
public record UserAccount
{
    public Guid Id { get; init; }

    public string Username { get; init; } = string.Empty;

    public string PasswordHash { get; init; } = string.Empty;

    public string Role { get; init; } = "User";

    public bool IsActive { get; init; } = true;
}