using StudentApi.Domain.Entities;

namespace StudentApi.Application.Interfaces;

/// <summary>
/// Persistence contract for refresh-token lifecycle operations.
/// </summary>
public interface IRefreshTokenRepository
{
    /// <summary>
    /// Stores a refresh token record.
    /// </summary>
    /// <param name="refreshToken">Refresh token entity to insert.</param>
    /// <param name="cancellationToken">Operation cancellation token.</param>
    /// <returns>A task that completes when insert finishes.</returns>
    Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an active refresh token by hash.
    /// </summary>
    /// <param name="tokenHash">Hashed token value.</param>
    /// <param name="cancellationToken">Operation cancellation token.</param>
    /// <returns>The active token record or <c>null</c>.</returns>
    Task<RefreshToken?> GetActiveByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default);

    /// <summary>
    /// Revokes an existing refresh token and records replacement hash.
    /// </summary>
    /// <param name="id">Refresh token identifier.</param>
    /// <param name="replacedByTokenHash">Hash of the token that replaced the revoked one.</param>
    /// <param name="cancellationToken">Operation cancellation token.</param>
    /// <returns>A task that completes when revocation finishes.</returns>
    Task RevokeAsync(Guid id, string replacedByTokenHash, CancellationToken cancellationToken = default);
}
