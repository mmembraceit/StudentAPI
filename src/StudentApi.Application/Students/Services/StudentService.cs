using StudentApi.Application.Interfaces;
using StudentApi.Application.Mappings;
using StudentApi.Domain.Entities;

namespace StudentApi.Application.Students;

public class StudentService : IStudentService
{
    private readonly IStudentRepository _studentRepository;

    public StudentService(IStudentRepository studentRepository)
    {
        _studentRepository = studentRepository;
    }

    public async Task<StudentDto?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        var student = await _studentRepository.GetByIdAsync(id, tenantId, cancellationToken);

        return student?.ToDto();
    }

    public async Task<IReadOnlyList<StudentDto>> GetAllAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var students = await _studentRepository.GetAllAsync(tenantId, cancellationToken);

        return students.Select(s => s.ToDto()).ToList();
    }

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

        return student.ToDto();
    }

    public async Task<StudentDto> UpdateAsync(Guid id, Guid tenantId, UpdateStudentRequest request, CancellationToken cancellationToken = default)
    {
        var currentStudent = await _studentRepository.GetByIdAsync(id, tenantId, cancellationToken);

        if (currentStudent is null)
        {
            throw new KeyNotFoundException($"Student with id '{id}' was not found for tenant '{tenantId}'.");
        }

        var updatedStudent = currentStudent with
        {
            Name = request.Name,
            DateOfBirth = request.DateOfBirth
        };

        await _studentRepository.UpdateAsync(updatedStudent, cancellationToken);

        return updatedStudent.ToDto();
    }

    public async Task DeleteAsync(DeleteStudentRequest request, CancellationToken cancellationToken = default)
    {
        var currentStudent = await _studentRepository.GetByIdAsync(request.Id, request.TenantId, cancellationToken);

        if (currentStudent is null)
        {
            throw new KeyNotFoundException($"Student with id '{request.Id}' was not found for tenant '{request.TenantId}'.");
        }

        await _studentRepository.DeleteAsync(request.Id, request.TenantId, cancellationToken);
    }
}
