using Microsoft.Extensions.Logging.Abstractions;
using StudentApi.Application.Students.Events;
using StudentApi.Infrastructure.Messaging;

namespace StudentApi.UnitTests.Infrastructure.Messaging;

public class NoOpStudentEventPublisherTests
{
    [Fact]
    public async Task PublishMethods_DoNotThrow()
    {
        var sut = new NoOpStudentEventPublisher(NullLogger<NoOpStudentEventPublisher>.Instance);
        var eventId = Guid.NewGuid();
        var studentId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var occurredAtUtc = DateTime.UtcNow;

        await sut.PublishCreatedAsync(new StudentCreatedIntegrationEvent(eventId, occurredAtUtc, studentId, tenantId, "Alice", new DateOnly(2001, 1, 1)));
        await sut.PublishUpdatedAsync(new StudentUpdatedIntegrationEvent(eventId, occurredAtUtc, studentId, tenantId, "Alice Updated", new DateOnly(2001, 1, 1)));
        await sut.PublishDeletedAsync(new StudentDeletedIntegrationEvent(eventId, occurredAtUtc, studentId, tenantId, "Alice Updated", new DateOnly(2001, 1, 1)));
    }
}
