using Microsoft.EntityFrameworkCore;
using StudentApi.Application.Interfaces;
using StudentApi.Domain.Entities;
using StudentApi.Infrastructure.Persistence;

namespace StudentApi.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IStudentRepository"/>.
/// </summary>
public class StudentRepository : IStudentRepository
{
    private readonly ApplicationDbContext _dbContext;

    public StudentRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Finds a student by id inside tenant scope.
    /// </summary>
    /// <param name="id">Student identifier.</param>
    /// <param name="tenantId">Tenant scope identifier.</param>
    /// <param name="cancellationToken">Operation cancellation token.</param>
    /// <returns>The matching student or <c>null</c> when not found.</returns>
    public async Task<Student?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Students
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id && s.TenantId == tenantId, cancellationToken);
    }

    /// <summary>
    /// Lists all students for a tenant ordered by name.
    /// </summary>
    /// <param name="tenantId">Tenant scope identifier.</param>
    /// <param name="cancellationToken">Operation cancellation token.</param>
    /// <returns>Tenant student collection.</returns>
    public async Task<IReadOnlyList<Student>> GetAllAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Students
            .AsNoTracking()
            .Where(s => s.TenantId == tenantId)
            .OrderBy(s => s.Name)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Inserts a new student and saves changes.
    /// </summary>
    /// <param name="student">Student entity to insert.</param>
    /// <param name="cancellationToken">Operation cancellation token.</param>
    /// <returns>A task that completes when insert finishes.</returns>
    public async Task AddAsync(Student student, CancellationToken cancellationToken = default)
    {
        await _dbContext.Students.AddAsync(student, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Updates an existing student and persists the change.
    /// </summary>
    /// <param name="student">Student entity with updated values.</param>
    /// <param name="cancellationToken">Operation cancellation token.</param>
    /// <returns>A task that completes when update finishes.</returns>
    public async Task UpdateAsync(Student student, CancellationToken cancellationToken = default)
    {
        _dbContext.Students.Update(student);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
    /// <summary>
    /// Deletes a student if it exists inside tenant scope.
    /// </summary>
    /// <param name="id">Student identifier.</param>
    /// <param name="tenantId">Tenant scope identifier.</param>
    /// <param name="cancellationToken">Operation cancellation token.</param>
    /// <returns>A task that completes when delete finishes.</returns>
    public async Task DeleteAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        var student = await _dbContext.Students
            .FirstOrDefaultAsync(s => s.Id == id && s.TenantId == tenantId, cancellationToken);

        if (student is null)
        {
            return;
        }

        _dbContext.Students.Remove(student);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
