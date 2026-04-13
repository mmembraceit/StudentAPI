namespace StudentApi.Domain.Entities;

/// <summary>
/// Domain entity that stores refresh-token lifecycle data.
/// - Refresh token identifier.
/// - Owning user account identifier.
/// - Username snapshot at token created time.
/// - Role snapshot at token created time.
/// - SHA-256 hash of the refresh token value.
/// - UTC timestamp when the token was created.
/// - UTC timestamp when the token expires.
/// - UTC timestamp when the token was revoked, if revoked.
/// - Hash of the token that replaced this token during rotation.
/// </summary>
public record RefreshToken
{
    public Guid Id { get; init; }

    public Guid UserAccountId { get; init; }

    public string Username { get; init; } = string.Empty;

    public string Role { get; init; } = string.Empty;

    public string TokenHash { get; init; } = string.Empty;

    public DateTime CreatedAtUtc { get; init; }

    public DateTime ExpiresAtUtc { get; init; }

    public DateTime? RevokedAtUtc { get; init; }

    public string? ReplacedByTokenHash { get; init; }
}
