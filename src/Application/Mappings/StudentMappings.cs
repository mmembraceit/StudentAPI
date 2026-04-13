using StudentApi.Application.Students;
using StudentApi.Domain.Entities;

namespace StudentApi.Application.Mappings;

/// Mapping extensions between domain entities and application DTOs.
public static class StudentMappings
{
    
    /// Converts a <see cref="Student"/> entity into a <see cref="StudentDto"/>.
    /// <returns>Mapped student DTO.</returns>
    public static StudentDto ToDto(this Student student)
    {
        return new StudentDto(
            student.Id,
            student.TenantId,
            student.Name,
            student.DateOfBirth);
    }
}
