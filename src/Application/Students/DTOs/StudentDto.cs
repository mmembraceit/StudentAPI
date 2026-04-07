namespace StudentApi.Application.Students;

/// <summary>
/// Output DTO used to expose student data outside the domain layer.
/// </summary>
/// <param name="Id">Student identifier.</param>
/// <param name="TenantId">Tenant scope identifier.</param>
/// <param name="Name">Student display name.</param>
/// <param name="DateOfBirth">Student birth date.</param>
public record StudentDto(
    Guid Id,
    Guid TenantId,
    string Name,
    DateOnly DateOfBirth);
