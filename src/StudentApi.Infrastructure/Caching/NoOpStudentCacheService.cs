using StudentApi.Application.Interfaces;
using StudentApi.Application.Students;

namespace StudentApi.Infrastructure.Caching;

public sealed class NoOpStudentCacheService : IStudentCacheService
{
    public Task<StudentDto?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default)
        => Task.FromResult<StudentDto?>(null);

    public Task SetByIdAsync(StudentDto student, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task<IReadOnlyList<StudentDto>?> GetAllAsync(Guid tenantId, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<StudentDto>?>(null);

    public Task SetAllAsync(Guid tenantId, IReadOnlyList<StudentDto> students, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task InvalidateByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task InvalidateAllAsync(Guid tenantId, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}
