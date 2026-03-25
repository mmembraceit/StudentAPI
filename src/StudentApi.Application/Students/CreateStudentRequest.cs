namespace StudentApi.Application.Students;

public record CreateStudentRequest(
    Guid TenantId,
    string Name,
    DateOnly DateOfBirth);
