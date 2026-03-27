namespace StudentApi.Application.Students;

public record DeleteStudentRequest(
    Guid Id,
    Guid TenantId);