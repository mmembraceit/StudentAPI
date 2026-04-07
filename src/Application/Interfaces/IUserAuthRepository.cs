using StudentApi.Domain.Entities;

namespace StudentApi.Application.Interfaces;

/// <summary>
/// Reads authentication user records from persistence.
/// </summary>
public interface IUserAuthRepository
{
    /// <summary>
    /// Gets an active user by username.
    /// </summary>
    /// <param name="username">Username to search.</param>
    /// <param name="cancellationToken">Operation cancellation token.</param>
    /// <returns>The matching active user account or <c>null</c>.</returns>
    Task<UserAccount?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
}