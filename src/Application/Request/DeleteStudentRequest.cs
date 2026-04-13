namespace StudentApi.Application.Students;


/// Request payload used to delete a student.
public record DeleteStudentRequest(
    Guid Id,
    Guid TenantId);