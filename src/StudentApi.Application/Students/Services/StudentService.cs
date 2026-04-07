using StudentApi.Application.Interfaces;
using StudentApi.Application.Common.Exceptions;
using StudentApi.Application.Mappings;
using StudentApi.Domain.Entities;

namespace StudentApi.Application.Students;


/// Implements the use cases for the Student module.
/// Belongs to the Application layer. It orchestrates repository access, mapping, and simple business rules.
/// It is invoked by <c>StudentsController</c> and relies on <c>IStudentRepository</c> for persistence.

public class StudentService : IStudentService
{
    private readonly IStudentRepository _studentRepository;
    private readonly IStudentCacheService _studentCacheService;

    public StudentService(IStudentRepository studentRepository, IStudentCacheService studentCacheService)
    {
        _studentRepository = studentRepository;
        _studentCacheService = studentCacheService;
    }

    
    /// Gets a student by id and tenant, or throws a business exception if it does not exist.
    
    public async Task<StudentDto> GetByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        var cachedStudent = await _studentCacheService.GetByIdAsync(id, tenantId, cancellationToken);

        if (cachedStudent is not null)
        {
            return cachedStudent;
        }

        var student = await _studentRepository.GetByIdAsync(id, tenantId, cancellationToken);

        if (student is null)
        {
            throw new NotFoundException($"Student with id '{id}' was not found for tenant '{tenantId}'.");
        }

        var studentDto = student.ToDto();
        await _studentCacheService.SetByIdAsync(studentDto, cancellationToken);

        return studentDto;
    }

    
    /// Returns all students visible for a tenant.
    
    public async Task<IReadOnlyList<StudentDto>> GetAllAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var cachedStudents = await _studentCacheService.GetAllAsync(tenantId, cancellationToken);

        if (cachedStudents is not null)
        {
            return cachedStudents;
        }

        var students = await _studentRepository.GetAllAsync(tenantId, cancellationToken);
        var studentDtos = students.Select(s => s.ToDto()).ToList();

        await _studentCacheService.SetAllAsync(tenantId, studentDtos, cancellationToken);

        return studentDtos;
    }

   
    /// Creates a new student and persists it in the database.
   
    public async Task<StudentDto> CreateAsync(CreateStudentRequest request, CancellationToken cancellationToken = default)
    {
        var student = new Student
        {
            Id = Guid.NewGuid(),
            TenantId = request.TenantId,
            Name = request.Name,
            DateOfBirth = request.DateOfBirth
        };

        await _studentRepository.AddAsync(student, cancellationToken);

        var studentDto = student.ToDto();
        await _studentCacheService.SetByIdAsync(studentDto, cancellationToken);
        await _studentCacheService.InvalidateAllAsync(student.TenantId, cancellationToken);

        return studentDto;
    }

   
    /// Updates an existing student inside the provided tenant.
    
    public async Task<StudentDto> UpdateAsync(Guid id, Guid tenantId, UpdateStudentRequest request, CancellationToken cancellationToken = default)
    {
        var currentStudent = await _studentRepository.GetByIdAsync(id, tenantId, cancellationToken);

        if (currentStudent is null)
        {
            throw new NotFoundException($"Student with id '{id}' was not found for tenant '{tenantId}'.");
        }

        var updatedStudent = currentStudent with
        {
            Name = request.Name,
            DateOfBirth = request.DateOfBirth
        };

        await _studentRepository.UpdateAsync(updatedStudent, cancellationToken);

        var studentDto = updatedStudent.ToDto();
        await _studentCacheService.SetByIdAsync(studentDto, cancellationToken);
        await _studentCacheService.InvalidateAllAsync(tenantId, cancellationToken);

        return studentDto;
    }

    
    /// Deletes an existing student inside the provided tenant.
    
    public async Task DeleteAsync(DeleteStudentRequest request, CancellationToken cancellationToken = default)
    {
        var currentStudent = await _studentRepository.GetByIdAsync(request.Id, request.TenantId, cancellationToken);

        if (currentStudent is null)
        {
            throw new NotFoundException($"Student with id '{request.Id}' was not found for tenant '{request.TenantId}'.");
        }

        await _studentRepository.DeleteAsync(request.Id, request.TenantId, cancellationToken);
        await _studentCacheService.InvalidateByIdAsync(request.Id, request.TenantId, cancellationToken);
        await _studentCacheService.InvalidateAllAsync(request.TenantId, cancellationToken);
    }
}
