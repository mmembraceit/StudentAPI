using StudentApi.Domain.Entities;

namespace StudentApi.Application.Interfaces;


/// Contract for student persistence operations.
public interface IStudentRepository
{
   
    /// Finds a student by id scoped to a tenant.
    /// <returns>The matching student or <c>null</c> when not found.</returns>
    Task<Student?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default);

  
    /// Gets all students for a tenant.
    /// <returns>Collection of students for the tenant.</returns>
    Task<IReadOnlyList<Student>> GetAllAsync(Guid tenantId, CancellationToken cancellationToken = default);

   
    /// Adds a student entity to persistence.
    /// <returns>A task that completes when insert finishes.</returns>
    Task AddAsync(Student student, CancellationToken cancellationToken = default);

    
    /// Updates an existing student entity.
    /// <returns>A task that completes when update finishes.</returns>
    Task UpdateAsync(Student student, CancellationToken cancellationToken = default);

 
    /// Deletes a student inside the provided tenant scope.
    /// <returns>A task that completes when delete finishes.</returns>
    Task DeleteAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default);
}