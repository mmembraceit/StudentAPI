namespace StudentApi.Domain.Entities;

/// Represents an authenticated API user account.
public record UserAccount
{
    public Guid Id { get; init; }

    public string Username { get; init; } = string.Empty;

    public string PasswordHash { get; init; } = string.Empty;

    public string Role { get; init; } = "User";

    public bool IsActive { get; init; } = true;
}