using StudentApi.Domain.Entities;

namespace StudentApi.Application.Interfaces;


/// Reads authentication user records from persistence.
public interface IUserAuthRepository
{

    /// Gets an active user by username.
    /// <returns>The matching active user account or <c>null</c>.</returns>
    Task<UserAccount?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
}