namespace StudentApi.Domain.Entities;

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
