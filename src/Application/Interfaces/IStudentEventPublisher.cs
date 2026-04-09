using StudentApi.Application.Students.Events;

namespace StudentApi.Application.Interfaces;

/// <summary>
/// Publishes integration events for student lifecycle changes.
/// </summary>
public interface IStudentEventPublisher
{
    /// <summary>
    /// Publishes a student-created integration event.
    /// </summary>
    /// <param name="integrationEvent">Event payload.</param>
    /// <param name="cancellationToken">Operation cancellation token.</param>
    /// <returns>A task that completes when the event has been published.</returns>
    Task PublishCreatedAsync(StudentCreatedIntegrationEvent integrationEvent, CancellationToken cancellationToken = default);

    /// <summary>
    /// Publishes a student-updated integration event.
    /// </summary>
    /// <param name="integrationEvent">Event payload.</param>
    /// <param name="cancellationToken">Operation cancellation token.</param>
    /// <returns>A task that completes when the event has been published.</returns>
    Task PublishUpdatedAsync(StudentUpdatedIntegrationEvent integrationEvent, CancellationToken cancellationToken = default);

    /// <summary>
    /// Publishes a student-deleted integration event.
    /// </summary>
    /// <param name="integrationEvent">Event payload.</param>
    /// <param name="cancellationToken">Operation cancellation token.</param>
    /// <returns>A task that completes when the event has been published.</returns>
    Task PublishDeletedAsync(StudentDeletedIntegrationEvent integrationEvent, CancellationToken cancellationToken = default);
}