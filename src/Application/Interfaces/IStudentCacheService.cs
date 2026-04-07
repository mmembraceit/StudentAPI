using StudentApi.Application.Students;

namespace StudentApi.Application.Interfaces;

/// <summary>
/// Defines cache operations used by student application use cases.
/// </summary>
public interface IStudentCacheService
{
    /// <summary>
    /// Gets a cached student by id and tenant.
    /// </summary>
    /// <param name="id">Student identifier.</param>
    /// <param name="tenantId">Tenant scope identifier.</param>
    /// <param name="cancellationToken">Operation cancellation token.</param>
    /// <returns>The cached student or <c>null</c> when not cached.</returns>
    Task<StudentDto?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Stores student cache by id.
    /// </summary>
    /// <param name="student">Student DTO to cache.</param>
    /// <param name="cancellationToken">Operation cancellation token.</param>
    /// <returns>A task that completes when cache write finishes.</returns>
    Task SetByIdAsync(StudentDto student, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets cached student list for a tenant.
    /// </summary>
    /// <param name="tenantId">Tenant scope identifier.</param>
    /// <param name="cancellationToken">Operation cancellation token.</param>
    /// <returns>Cached student list or <c>null</c> when cache miss.</returns>
    Task<IReadOnlyList<StudentDto>?> GetAllAsync(Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Stores tenant student list in cache.
    /// </summary>
    /// <param name="tenantId">Tenant scope identifier.</param>
    /// <param name="students">Student DTO list to cache.</param>
    /// <param name="cancellationToken">Operation cancellation token.</param>
    /// <returns>A task that completes when cache write finishes.</returns>
    Task SetAllAsync(Guid tenantId, IReadOnlyList<StudentDto> students, CancellationToken cancellationToken = default);

    /// <summary>
    /// Invalidates cached student-by-id entry.
    /// </summary>
    /// <param name="id">Student identifier.</param>
    /// <param name="tenantId">Tenant scope identifier.</param>
    /// <param name="cancellationToken">Operation cancellation token.</param>
    /// <returns>A task that completes when cache delete finishes.</returns>
    Task InvalidateByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Invalidates cached tenant student-list entry.
    /// </summary>
    /// <param name="tenantId">Tenant scope identifier.</param>
    /// <param name="cancellationToken">Operation cancellation token.</param>
    /// <returns>A task that completes when cache delete finishes.</returns>
    Task InvalidateAllAsync(Guid tenantId, CancellationToken cancellationToken = default);
}