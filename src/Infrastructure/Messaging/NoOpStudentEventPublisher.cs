using Microsoft.Extensions.Logging;
using StudentApi.Application.Interfaces;
using StudentApi.Application.Students.Events;

namespace StudentApi.Infrastructure.Messaging;

/// <summary>
/// No-op student event publisher used when Azure Service Bus is not configured.
/// </summary>
public sealed class NoOpStudentEventPublisher : IStudentEventPublisher
{
    private readonly ILogger<NoOpStudentEventPublisher> _logger;

    public NoOpStudentEventPublisher(ILogger<NoOpStudentEventPublisher> logger)
    {
        _logger = logger;
    }

    public Task PublishCreatedAsync(StudentCreatedIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
        => LogSkipAsync("student.created", integrationEvent.StudentId, integrationEvent.TenantId);

    public Task PublishUpdatedAsync(StudentUpdatedIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
        => LogSkipAsync("student.updated", integrationEvent.StudentId, integrationEvent.TenantId);

    public Task PublishDeletedAsync(StudentDeletedIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
        => LogSkipAsync("student.deleted", integrationEvent.StudentId, integrationEvent.TenantId);

    private Task LogSkipAsync(string subject, Guid studentId, Guid tenantId)
    {
        _logger.LogInformation("SERVICE BUS SKIP {Subject} student:{StudentId} tenant:{TenantId}", subject, studentId, tenantId);
        return Task.CompletedTask;
    }
}