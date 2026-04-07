using Microsoft.EntityFrameworkCore;
using StudentApi.Application.Interfaces;
using StudentApi.Domain.Entities;
using StudentApi.Infrastructure.Persistence;

namespace StudentApi.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of refresh-token persistence operations.
/// </summary>
public sealed class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly ApplicationDbContext _dbContext;

    public RefreshTokenRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Stores a refresh token entity.
    /// </summary>
    /// <param name="refreshToken">Refresh token entity to insert.</param>
    /// <param name="cancellationToken">Operation cancellation token.</param>
    /// <returns>A task that completes when insert finishes.</returns>
    public async Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
    {
        await _dbContext.RefreshTokens.AddAsync(refreshToken, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Gets an active refresh token by token hash.
    /// </summary>
    /// <param name="tokenHash">Hashed token value.</param>
    /// <param name="cancellationToken">Operation cancellation token.</param>
    /// <returns>The active refresh-token entity or <c>null</c>.</returns>
    public async Task<RefreshToken?> GetActiveByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        return await _dbContext.RefreshTokens
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.TokenHash == tokenHash
                    && x.RevokedAtUtc == null
                    && x.ExpiresAtUtc > now,
                cancellationToken);
    }

    /// <summary>
    /// Revokes a refresh token and records the replacement token hash.
    /// </summary>
    /// <param name="id">Refresh token identifier.</param>
    /// <param name="replacedByTokenHash">Replacement token hash.</param>
    /// <param name="cancellationToken">Operation cancellation token.</param>
    /// <returns>A task that completes when update finishes.</returns>
    public async Task RevokeAsync(Guid id, string replacedByTokenHash, CancellationToken cancellationToken = default)
    {
        var existing = await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (existing is null)
        {
            return;
        }

        var updated = existing with
        {
            RevokedAtUtc = DateTime.UtcNow,
            ReplacedByTokenHash = replacedByTokenHash
        };

        _dbContext.Entry(existing).CurrentValues.SetValues(updated);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
