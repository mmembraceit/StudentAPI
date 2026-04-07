using Microsoft.EntityFrameworkCore;
using StudentApi.Application.Interfaces;
using StudentApi.Domain.Entities;
using StudentApi.Infrastructure.Persistence;

namespace StudentApi.Infrastructure.Repositories;

public sealed class UserAuthRepository : IUserAuthRepository
{
    private readonly ApplicationDbContext _dbContext;

    public UserAuthRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<UserAccount?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        return await _dbContext.UserAccounts
            .AsNoTracking()
            .FirstOrDefaultAsync(
                user => user.Username == username && user.IsActive,
                cancellationToken);
    }
}
