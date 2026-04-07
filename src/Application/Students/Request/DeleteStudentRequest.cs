namespace StudentApi.Application.Students;

/// <summary>
/// Request payload used to delete a student.
/// </summary>
/// <param name="Id">Student identifier.</param>
/// <param name="TenantId">Tenant scope identifier.</param>
public record DeleteStudentRequest(
    Guid Id,
    Guid TenantId);