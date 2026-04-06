using StudentApi.Application.Students;

namespace StudentApi.Application.Interfaces;

/// Application service contract for student use cases.
/// Consumed by the Presentation layer from <c>StudentsController</c>.
/// Internally it relies on <c>IStudentRepository</c> and DTO mappings to decouple HTTP from business logic.

public interface IStudentService
{
    Task<StudentDto> GetByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<StudentDto>> GetAllAsync(Guid tenantId, CancellationToken cancellationToken = default);

    Task<StudentDto> CreateAsync(CreateStudentRequest request, CancellationToken cancellationToken = default);

    Task<StudentDto> UpdateAsync(Guid id, Guid tenantId, UpdateStudentRequest request, CancellationToken cancellationToken = default);

    Task DeleteAsync(DeleteStudentRequest request, CancellationToken cancellationToken = default);
}
