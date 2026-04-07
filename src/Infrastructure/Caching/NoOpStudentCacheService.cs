using StudentApi.Application.Interfaces;
using StudentApi.Application.Students;

namespace StudentApi.Infrastructure.Caching;

/// <summary>
/// No-op cache implementation used when Redis is not configured.
/// </summary>
public sealed class NoOpStudentCacheService : IStudentCacheService
{
    /// <summary>
    /// Always returns no cached item.
    /// </summary>
    /// <param name="id">Student identifier.</param>
    /// <param name="tenantId">Tenant scope identifier.</param>
    /// <param name="cancellationToken">Operation cancellation token.</param>
    /// <returns>Always <c>null</c>.</returns>
    public Task<StudentDto?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default)
        => Task.FromResult<StudentDto?>(null);

    /// <summary>
    /// No-op write for by-id cache.
    /// </summary>
    /// <param name="student">Student DTO to cache.</param>
    /// <param name="cancellationToken">Operation cancellation token.</param>
    /// <returns>A completed task.</returns>
    public Task SetByIdAsync(StudentDto student, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    /// <summary>
    /// Always returns no cached list.
    /// </summary>
    /// <param name="tenantId">Tenant scope identifier.</param>
    /// <param name="cancellationToken">Operation cancellation token.</param>
    /// <returns>Always <c>null</c>.</returns>
    public Task<IReadOnlyList<StudentDto>?> GetAllAsync(Guid tenantId, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<StudentDto>?>(null);

    /// <summary>
    /// No-op write for tenant list cache.
    /// </summary>
    /// <param name="tenantId">Tenant scope identifier.</param>
    /// <param name="students">Student DTO list.</param>
    /// <param name="cancellationToken">Operation cancellation token.</param>
    /// <returns>A completed task.</returns>
    public Task SetAllAsync(Guid tenantId, IReadOnlyList<StudentDto> students, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    /// <summary>
    /// No-op invalidate for by-id cache.
    /// </summary>
    /// <param name="id">Student identifier.</param>
    /// <param name="tenantId">Tenant scope identifier.</param>
    /// <param name="cancellationToken">Operation cancellation token.</param>
    /// <returns>A completed task.</returns>
    public Task InvalidateByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    /// <summary>
    /// No-op invalidate for tenant list cache.
    /// </summary>
    /// <param name="tenantId">Tenant scope identifier.</param>
    /// <param name="cancellationToken">Operation cancellation token.</param>
    /// <returns>A completed task.</returns>
    public Task InvalidateAllAsync(Guid tenantId, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}
