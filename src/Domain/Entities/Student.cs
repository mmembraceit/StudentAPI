namespace StudentApi.Domain.Entities;

/// <summary>
/// Domain entity that represents a student.
/// - Student identifier.
/// - Tenant scope identifier.
/// - Student display name.
/// - Student birth date.
/// </summary>
public record Student
{
    public Guid Id { get; init; }

    public Guid TenantId { get; init; }

    public string Name { get; init; } = string.Empty;

    public DateOnly DateOfBirth { get; init; }
}