namespace StudentApi.Application.Students;


/// Request payload used to update a student.
public record UpdateStudentRequest(
    string Name,
    DateOnly DateOfBirth);
