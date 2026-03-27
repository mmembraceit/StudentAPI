namespace StudentApi.Application.Students;

public record UpdateStudentRequest(
    string Name,
    DateOnly DateOfBirth);
