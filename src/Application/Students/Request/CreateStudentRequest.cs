namespace StudentApi.Application.Students;

/// <summary>
/// Request payload used to create a student.
/// </summary>
/// <param name="TenantId">Tenant scope identifier.</param>
/// <param name="Name">Student display name.</param>
/// <param name="DateOfBirth">Student birth date.</param>
public record CreateStudentRequest(
    Guid TenantId,
    string Name,
    DateOnly DateOfBirth);
