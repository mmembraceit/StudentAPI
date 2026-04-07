using StudentApi.Application.Students;

namespace StudentApi.Application.Interfaces;

/// Defines the cache operations used by the application service
public interface IStudentCacheService
{
    Task<StudentDto?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default);

    Task SetByIdAsync(StudentDto student, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<StudentDto>?> GetAllAsync(Guid tenantId, CancellationToken cancellationToken = default);

    Task SetAllAsync(Guid tenantId, IReadOnlyList<StudentDto> students, CancellationToken cancellationToken = default);

    Task InvalidateByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default);

    Task InvalidateAllAsync(Guid tenantId, CancellationToken cancellationToken = default);
}