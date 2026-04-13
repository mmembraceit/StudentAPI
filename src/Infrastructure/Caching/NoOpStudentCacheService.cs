using StudentApi.Application.Interfaces;
using StudentApi.Application.Students;

namespace StudentApi.Infrastructure.Caching;


/// No-op cache implementation used when Redis is not configured.
public sealed class NoOpStudentCacheService : IStudentCacheService
{
 
    /// Always returns no cached item.
    /// <returns>Always <c>null</c>.</returns>
    public Task<StudentDto?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default)
        => Task.FromResult<StudentDto?>(null);

  
    /// No-op write for by-id cache.
    /// <returns>A completed task.</returns>
    public Task SetByIdAsync(StudentDto student, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

  
    /// Always returns no cached list.
    /// <returns>Always <c>null</c>.</returns>
    public Task<IReadOnlyList<StudentDto>?> GetAllAsync(Guid tenantId, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<StudentDto>?>(null);

 
    /// No-op write for tenant list cache.
    /// <returns>A completed task.</returns>
    public Task SetAllAsync(Guid tenantId, IReadOnlyList<StudentDto> students, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    /// No-op invalidate for by-id cache.
    /// <returns>A completed task.</returns>
    public Task InvalidateByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default)
        => Task.CompletedTask;


    /// No-op invalidate for tenant list cache.
    /// <returns>A completed task.</returns>
    public Task InvalidateAllAsync(Guid tenantId, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}
