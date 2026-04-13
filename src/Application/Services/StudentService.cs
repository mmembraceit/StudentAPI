using StudentApi.Application.Interfaces;
using StudentApi.Application.Common.Exceptions;
using StudentApi.Application.Mappings;
using StudentApi.Application.Students.Events;
using StudentApi.Domain.Entities;

namespace StudentApi.Application.Students;

/// <summary>
/// Implements student use cases in the application layer.
/// </summary>
public class StudentService : IStudentService
{
    private readonly IStudentRepository _studentRepository;
    private readonly IStudentCacheService _studentCacheService;
    private readonly IStudentEventPublisher _studentEventPublisher;

    public StudentService(
        IStudentRepository studentRepository,
        IStudentCacheService studentCacheService,
        IStudentEventPublisher studentEventPublisher)
    {
        _studentRepository = studentRepository;
        _studentCacheService = studentCacheService;
        _studentEventPublisher = studentEventPublisher;
    }

    /// <summary>
    /// Gets a student by id and tenant, using cache first and repository fallback.
    /// </summary>
    /// <param name="id">Student identifier.</param>
    /// <param name="tenantId">Tenant scope identifier.</param>
    /// <param name="cancellationToken">Operation cancellation token.</param>
    /// <returns>The matching student DTO.</returns>
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

    /// <summary>
    /// Gets all students for a tenant, using cache first and repository fallback.
    /// </summary>
    /// <param name="tenantId">Tenant scope identifier.</param>
    /// <param name="cancellationToken">Operation cancellation token.</param>
    /// <returns>Student DTO list for the tenant.</returns>
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

    /// <summary>
    /// Creates a new student, stores it, then updates related cache entries.
    /// </summary>
    /// <param name="request">Student creation payload.</param>
    /// <param name="cancellationToken">Operation cancellation token.</param>
    /// <returns>The created student DTO.</returns>
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
        await _studentEventPublisher.PublishCreatedAsync(
            new StudentCreatedIntegrationEvent(
                Guid.NewGuid(),
                DateTime.UtcNow,
                studentDto.Id,
                studentDto.TenantId,
                studentDto.Name,
                studentDto.DateOfBirth),
            cancellationToken);

        return studentDto;
    }

    /// <summary>
    /// Updates an existing student inside tenant scope and refreshes cache entries.
    /// </summary>
    /// <param name="id">Student identifier.</param>
    /// <param name="tenantId">Tenant scope identifier.</param>
    /// <param name="request">Student update payload.</param>
    /// <param name="cancellationToken">Operation cancellation token.</param>
    /// <returns>The updated student DTO.</returns>
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
        await _studentEventPublisher.PublishUpdatedAsync(
            new StudentUpdatedIntegrationEvent(
                Guid.NewGuid(),
                DateTime.UtcNow,
                studentDto.Id,
                studentDto.TenantId,
                studentDto.Name,
                studentDto.DateOfBirth),
            cancellationToken);

        return studentDto;
    }

    /// <summary>
    /// Deletes an existing student inside tenant scope and invalidates related cache entries.
    /// </summary>
    /// <param name="request">Delete payload with student and tenant identifiers.</param>
    /// <param name="cancellationToken">Operation cancellation token.</param>
    /// <returns>A task that completes when delete finishes.</returns>
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
        await _studentEventPublisher.PublishDeletedAsync(
            new StudentDeletedIntegrationEvent(
                Guid.NewGuid(),
                DateTime.UtcNow,
                currentStudent.Id,
                currentStudent.TenantId,
                currentStudent.Name,
                currentStudent.DateOfBirth),
            cancellationToken);
    }
}
