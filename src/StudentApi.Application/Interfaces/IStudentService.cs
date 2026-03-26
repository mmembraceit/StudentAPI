using StudentApi.Application.Students;

namespace StudentApi.Application.Interfaces;

public interface IStudentService
{
    Task<StudentDto?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<StudentDto>> GetAllAsync(Guid tenantId, CancellationToken cancellationToken = default);

    Task<StudentDto> CreateAsync(CreateStudentRequest request, CancellationToken cancellationToken = default);

    Task<StudentDto> UpdateAsync(Guid id, Guid tenantId, UpdateStudentRequest request, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default);
}
