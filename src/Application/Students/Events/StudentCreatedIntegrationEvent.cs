namespace StudentApi.Application.Students.Events;

/// <summary>
/// Integration event emitted when a student is created.
/// </summary>
public sealed record StudentCreatedIntegrationEvent(
    Guid EventId,
    DateTime OccurredAtUtc,
    Guid StudentId,
    Guid TenantId,
    string Name,
    DateOnly DateOfBirth);