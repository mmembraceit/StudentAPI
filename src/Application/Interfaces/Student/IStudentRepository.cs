using StudentApi.Domain.Entities;

namespace StudentApi.Application.Interfaces;

/// <summary>
/// Contract for student persistence operations.
/// </summary>
public interface IStudentRepository
{
    /// <summary>
    /// Finds a student by id scoped to a tenant.
    /// </summary>
    /// <param name="id">Student identifier.</param>
    /// <param name="tenantId">Tenant scope identifier.</param>
    /// <param name="cancellationToken">Operation cancellation token.</param>
    /// <returns>The matching student or <c>null</c> when not found.</returns>
    Task<Student?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all students for a tenant.
    /// </summary>
    /// <param name="tenantId">Tenant scope identifier.</param>
    /// <param name="cancellationToken">Operation cancellation token.</param>
    /// <returns>Collection of students for the tenant.</returns>
    Task<IReadOnlyList<Student>> GetAllAsync(Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a student entity to persistence.
    /// </summary>
    /// <param name="student">Student entity to insert.</param>
    /// <param name="cancellationToken">Operation cancellation token.</param>
    /// <returns>A task that completes when insert finishes.</returns>
    Task AddAsync(Student student, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing student entity.
    /// </summary>
    /// <param name="student">Student entity with updated values.</param>
    /// <param name="cancellationToken">Operation cancellation token.</param>
    /// <returns>A task that completes when update finishes.</returns>
    Task UpdateAsync(Student student, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a student inside the provided tenant scope.
    /// </summary>
    /// <param name="id">Student identifier.</param>
    /// <param name="tenantId">Tenant scope identifier.</param>
    /// <param name="cancellationToken">Operation cancellation token.</param>
    /// <returns>A task that completes when delete finishes.</returns>
    Task DeleteAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default);
}