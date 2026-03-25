namespace StudentApi.Domain.Entities;

public record Student
{
    public Guid Id { get; init; }

    public Guid TenantId { get; init; }

    public string Name { get; init; } = string.Empty;

    public DateOnly DateOfBirth { get; init; }
}