using StudentApi.Domain.Entities;

namespace StudentApi.Application.Interfaces;

public interface IUserAuthRepository
{
    Task<UserAccount?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
}