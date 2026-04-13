namespace StudentApi.Application.Students;

/// Output DTO used to expose student data outside the domain layer.
public record StudentDto(
    Guid Id,
    Guid TenantId,
    string Name,
    DateOnly DateOfBirth);
