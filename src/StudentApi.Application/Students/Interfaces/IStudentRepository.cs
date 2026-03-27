using StudentApi.Domain.Entities;

namespace StudentApi.Application.Interfaces;

public interface IStudentRepository
{
    Task<Student?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Student>> GetAllAsync(Guid tenantId, CancellationToken cancellationToken = default);

    Task AddAsync(Student student, CancellationToken cancellationToken = default);

    Task UpdateAsync(Student student, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default);
}