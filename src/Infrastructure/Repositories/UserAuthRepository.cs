using Microsoft.EntityFrameworkCore;
using StudentApi.Application.Interfaces;
using StudentApi.Domain.Entities;
using StudentApi.Infrastructure.Persistence;

namespace StudentApi.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation for querying authentication users.
/// </summary>
public sealed class UserAuthRepository : IUserAuthRepository
{
    private readonly ApplicationDbContext _dbContext;

    public UserAuthRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Gets an active user account by username.
    /// </summary>
    /// <param name="username">Username to search.</param>
    /// <param name="cancellationToken">Operation cancellation token.</param>
    /// <returns>The matching active user account or <c>null</c>.</returns>
    public async Task<UserAccount?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        return await _dbContext.UserAccounts
            .AsNoTracking()
            .FirstOrDefaultAsync(
                user => user.Username == username && user.IsActive,
                cancellationToken);
    }
}
