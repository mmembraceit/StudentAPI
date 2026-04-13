using StudentApi.Application.Students;

namespace StudentApi.Application.Interfaces;

/// <summary>
/// Application service contract for student use cases.
/// </summary>
public interface IStudentService
{
    /// <summary>
    /// Gets a student by id and tenant.
    /// </summary>
    /// <param name="id">Student identifier.</param>
    /// <param name="tenantId">Tenant scope identifier.</param>
    /// <param name="cancellationToken">Operation cancellation token.</param>
    /// <returns>The matching student DTO.</returns>
    Task<StudentDto> GetByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all students visible for a tenant.
    /// </summary>
    /// <param name="tenantId">Tenant scope identifier.</param>
    /// <param name="cancellationToken">Operation cancellation token.</param>
    /// <returns>Student DTO collection for the tenant.</returns>
    Task<IReadOnlyList<StudentDto>> GetAllAsync(Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new student.
    /// </summary>
    /// <param name="request">Creation payload.</param>
    /// <param name="cancellationToken">Operation cancellation token.</param>
    /// <returns>The created student DTO.</returns>
    Task<StudentDto> CreateAsync(CreateStudentRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing student.
    /// </summary>
    /// <param name="id">Student identifier.</param>
    /// <param name="tenantId">Tenant scope identifier.</param>
    /// <param name="request">Update payload.</param>
    /// <param name="cancellationToken">Operation cancellation token.</param>
    /// <returns>The updated student DTO.</returns>
    Task<StudentDto> UpdateAsync(Guid id, Guid tenantId, UpdateStudentRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an existing student.
    /// </summary>
    /// <param name="request">Delete payload with id and tenant scope.</param>
    /// <param name="cancellationToken">Operation cancellation token.</param>
    /// <returns>A task that completes when delete finishes.</returns>
    Task DeleteAsync(DeleteStudentRequest request, CancellationToken cancellationToken = default);
}
