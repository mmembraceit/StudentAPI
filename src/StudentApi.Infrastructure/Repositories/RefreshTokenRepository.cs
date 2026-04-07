using Microsoft.EntityFrameworkCore;
using StudentApi.Application.Interfaces;
using StudentApi.Domain.Entities;
using StudentApi.Infrastructure.Persistence;

namespace StudentApi.Infrastructure.Repositories;

public sealed class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly ApplicationDbContext _dbContext;

    public RefreshTokenRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
    {
        await _dbContext.RefreshTokens.AddAsync(refreshToken, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

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
