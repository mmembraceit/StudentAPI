using StudentApi.Application.Students;

namespace StudentApi.Application.Interfaces;


/// Defines cache operations used by student application use cases.
public interface IStudentCacheService
{
   
    /// Gets a cached student by id and tenant.
    /// <returns>The cached student or <c>null</c> when not cached.</returns>
    Task<StudentDto?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default);

  
    /// Stores student cache by id.
    /// <returns>A task that completes when cache write finishes.</returns>
    Task SetByIdAsync(StudentDto student, CancellationToken cancellationToken = default);

  
    /// Gets cached student list for a tenant.
    /// <returns>Cached student list or <c>null</c> when cache miss.</returns>
    Task<IReadOnlyList<StudentDto>?> GetAllAsync(Guid tenantId, CancellationToken cancellationToken = default);

    /// Stores tenant student list in cache.
    /// <returns>A task that completes when cache write finishes.</returns>
    Task SetAllAsync(Guid tenantId, IReadOnlyList<StudentDto> students, CancellationToken cancellationToken = default);

  
    /// Invalidates cached student-by-id entry.
    /// <returns>A task that completes when cache delete finishes.</returns>
    Task InvalidateByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default);

  
    /// Invalidates cached tenant student-list entry.
    /// <returns>A task that completes when cache delete finishes.</returns>
    Task InvalidateAllAsync(Guid tenantId, CancellationToken cancellationToken = default);
}