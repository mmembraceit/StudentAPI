namespace StudentApi.Application.Students;


/// Request payload used to create a student.
public record CreateStudentRequest(
    Guid TenantId,
    string Name,
    DateOnly DateOfBirth);
