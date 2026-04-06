namespace StudentApi.Domain.Entities;

/// Main domain entity for the student module.
/// Lives in the Domain layer and represents pure business data, without HTTP, EF Core, or infrastructure dependencies.
/// It is related to <c>StudentDto</c> through the Application mapper and to the <c>Students</c> table through the Infrastructure configuration.

public record Student
{
    public Guid Id { get; init; }

    public Guid TenantId { get; init; }

    public string Name { get; init; } = string.Empty;

    public DateOnly DateOfBirth { get; init; }
}