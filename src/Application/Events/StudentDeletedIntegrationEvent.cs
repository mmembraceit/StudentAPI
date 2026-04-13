namespace StudentApi.Application.Students.Events;

/// <summary>
/// Integration event emitted when a student is deleted.
/// </summary>
public sealed record StudentDeletedIntegrationEvent(
    Guid EventId,
    DateTime OccurredAtUtc,
    Guid StudentId,
    Guid TenantId,
    string Name,
    DateOnly DateOfBirth);