using StudentApi.Domain.Entities;

namespace StudentApi.Application.Interfaces;

/// <summary>
/// Persistence contract for refresh-token lifecycle operations.
/// </summary>
public interface IRefreshTokenRepository
{
   
    /// Stores a refresh token record.
    /// <returns>A task that completes when insert finishes.</returns>
    Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);


    /// Gets an active refresh token by hash.
    /// <returns>The active token record or <c>null</c>.</returns>
    Task<RefreshToken?> GetActiveByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default);

  
    /// Revokes an existing refresh token and records replacement hash.
    /// <returns>A task that completes when revocation finishes.</returns>
    Task RevokeAsync(Guid id, string replacedByTokenHash, CancellationToken cancellationToken = default);
}
