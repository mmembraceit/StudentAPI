namespace StudentApi.Application.Students;

/// <summary>
/// Request payload used to update a student.
/// </summary>
/// <param name="Name">Updated student display name.</param>
/// <param name="DateOfBirth">Updated student birth date.</param>
public record UpdateStudentRequest(
    string Name,
    DateOnly DateOfBirth);
