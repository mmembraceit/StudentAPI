namespace StudentApi.Application.Students;


/// Output DTO used to expose students outside the domain layer.
/// Returned by <c>StudentService</c> and travels up to the API through <c>StudentsController</c>.
/// It avoids exposing the domain entity <c>Student</c> directly.

public record StudentDto(
    Guid Id,
    Guid TenantId,
    string Name,
    DateOnly DateOfBirth);
