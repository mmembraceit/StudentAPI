using StudentApi.Application.Students;
using StudentApi.Domain.Entities;

namespace StudentApi.Application.Mappings;

/// <summary>
/// Mapping extensions between domain entities and application DTOs.
/// </summary>

public static class StudentMappings
{
    /// <summary>
    /// Converts a <see cref="Student"/> entity into a <see cref="StudentDto"/>.
    /// </summary>
    /// <param name="student">Student entity source.</param>
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
