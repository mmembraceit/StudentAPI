namespace StudentApi.Application.Students.Events;

/// <summary>
/// Integration event emitted when a student is updated.
/// </summary>
public sealed record StudentUpdatedIntegrationEvent(
    Guid EventId,
    DateTime OccurredAtUtc,
    Guid StudentId,
    Guid TenantId,
    string Name,
    DateOnly DateOfBirth);