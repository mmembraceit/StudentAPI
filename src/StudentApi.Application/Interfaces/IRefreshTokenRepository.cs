using StudentApi.Domain.Entities;

namespace StudentApi.Application.Interfaces;

public interface IRefreshTokenRepository
{
    Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);

    Task<RefreshToken?> GetActiveByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default);

    Task RevokeAsync(Guid id, string replacedByTokenHash, CancellationToken cancellationToken = default);
}
