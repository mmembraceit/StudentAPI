using StudentApi.Application.Students.Events;

namespace StudentApi.Application.Interfaces;


/// Publishes integration events for student lifecycle changes.
public interface IStudentEventPublisher
{
    /// Publishes a student-created integration event.
    /// <returns>A task that completes when the event has been published.</returns>
    Task PublishCreatedAsync(StudentCreatedIntegrationEvent integrationEvent, CancellationToken cancellationToken = default);

    /// Publishes a student-updated integration event.
    /// <returns>A task that completes when the event has been published.</returns>
    Task PublishUpdatedAsync(StudentUpdatedIntegrationEvent integrationEvent, CancellationToken cancellationToken = default);

    /// Publishes a student-deleted integration event.
    /// <returns>A task that completes when the event has been published.</returns>
    Task PublishDeletedAsync(StudentDeletedIntegrationEvent integrationEvent, CancellationToken cancellationToken = default);
}