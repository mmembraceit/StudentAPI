namespace StudentApi.Application.Students;

public record StudentDto(
    Guid Id,
    Guid TenantId,
    string Name,
    DateOnly DateOfBirth);
