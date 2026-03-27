using StudentApi.Application.Students;
using StudentApi.Domain.Entities;

namespace StudentApi.Application.Mappings;

public static class StudentMappings
{
    public static StudentDto ToDto(this Student student)
    {
        return new StudentDto(
            student.Id,
            student.TenantId,
            student.Name,
            student.DateOfBirth);
    }
}
