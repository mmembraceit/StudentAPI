using StudentApi.Domain.Entities;

namespace StudentApi.Application.Interfaces;

/// Contract for the student repository.
/// Defined in Application so business logic depends on an abstraction instead of EF Core.
/// Its concrete implementation lives in <c>StudentRepository</c> inside Infrastructure.

public interface IStudentRepository
{
    Task<Student?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Student>> GetAllAsync(Guid tenantId, CancellationToken cancellationToken = default);

    Task AddAsync(Student student, CancellationToken cancellationToken = default);

    Task UpdateAsync(Student student, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default);
}