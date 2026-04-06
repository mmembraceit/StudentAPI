using StudentApi.Application.Students;
using StudentApi.Domain.Entities;

namespace StudentApi.Application.Mappings;

/// Mappings between domain models and application DTOs.
/// Centralizes conversion so neither the controller nor the repository needs to know the output format.

public static class StudentMappings
{

    /// EN: Converts the <c>Student</c> domain entity into <c>StudentDto</c>.
  
    public static StudentDto ToDto(this Student student)
    {
        return new StudentDto(
            student.Id,
            student.TenantId,
            student.Name,
            student.DateOfBirth);
    }
}
