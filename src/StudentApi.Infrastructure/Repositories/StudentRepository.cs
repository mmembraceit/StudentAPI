using Microsoft.EntityFrameworkCore;
using StudentApi.Application.Interfaces;
using StudentApi.Domain.Entities;
using StudentApi.Infrastructure.Persistence;

namespace StudentApi.Infrastructure.Repositories;


/// EF Core implementation of the student repository.
/// Materializes the <c>IStudentRepository</c> contract and encapsulates queries over <c>ApplicationDbContext</c>.

public class StudentRepository : IStudentRepository
{
    private readonly ApplicationDbContext _dbContext;

    public StudentRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    
    /// Finds a student by id inside the provided tenant.
    
    public async Task<Student?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Students
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id && s.TenantId == tenantId, cancellationToken);
    }

   
    /// Lists all students for the tenant, ordered by name.
    
    public async Task<IReadOnlyList<Student>> GetAllAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Students
            .AsNoTracking()
            .Where(s => s.TenantId == tenantId)
            .OrderBy(s => s.Name)
            .ToListAsync(cancellationToken);
    }

    
    /// Inserts a new student and saves changes immediately.
   
    public async Task AddAsync(Student student, CancellationToken cancellationToken = default)
    {
        await _dbContext.Students.AddAsync(student, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    
    /// Updates an existing student and persists the change.
  
    public async Task UpdateAsync(Student student, CancellationToken cancellationToken = default)
    {
        _dbContext.Students.Update(student);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
    
  
    /// Deletes a student if it exists inside the provided tenant.
   
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
