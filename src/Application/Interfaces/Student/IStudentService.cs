using StudentApi.Application.Students;

namespace StudentApi.Application.Interfaces;

/// Application service contract for student use cases.
public interface IStudentService
{
  
    /// Gets a student by id and tenant.
    /// <returns>The matching student DTO.</returns>
    Task<StudentDto> GetByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default);

  
    /// Gets all students visible for a tenant.
    /// <returns>Student DTO collection for the tenant.</returns>
    Task<IReadOnlyList<StudentDto>> GetAllAsync(Guid tenantId, CancellationToken cancellationToken = default);


    /// Creates a new student.
    /// <returns>The created student DTO.</returns>
    Task<StudentDto> CreateAsync(CreateStudentRequest request, CancellationToken cancellationToken = default);

    /// Updates an existing student.
    /// <returns>The updated student DTO.</returns>
    Task<StudentDto> UpdateAsync(Guid id, Guid tenantId, UpdateStudentRequest request, CancellationToken cancellationToken = default);

  
    /// Deletes an existing student.
    /// <returns>A task that completes when delete finishes.</returns>
    Task DeleteAsync(DeleteStudentRequest request, CancellationToken cancellationToken = default);
}
