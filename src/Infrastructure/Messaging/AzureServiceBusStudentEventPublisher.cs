using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using StudentApi.Application.Interfaces;
using StudentApi.Application.Students.Events;

namespace StudentApi.Infrastructure.Messaging;

/// <summary>
/// Azure Service Bus publisher for student integration events.
/// </summary>
public sealed class AzureServiceBusStudentEventPublisher : IStudentEventPublisher, IAsyncDisposable
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private readonly ServiceBusSender _sender;
    private readonly ILogger<AzureServiceBusStudentEventPublisher> _logger;

    public AzureServiceBusStudentEventPublisher(ServiceBusClient serviceBusClient, AzureServiceBusOptions options, ILogger<AzureServiceBusStudentEventPublisher> logger)
    {
        _sender = serviceBusClient.CreateSender(options.QueueName);
        _logger = logger;
    }

    public Task PublishCreatedAsync(StudentCreatedIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
        => PublishAsync("student.created", integrationEvent, integrationEvent.EventId, integrationEvent.StudentId, integrationEvent.TenantId, cancellationToken);

    public Task PublishUpdatedAsync(StudentUpdatedIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
        => PublishAsync("student.updated", integrationEvent, integrationEvent.EventId, integrationEvent.StudentId, integrationEvent.TenantId, cancellationToken);

    public Task PublishDeletedAsync(StudentDeletedIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
        => PublishAsync("student.deleted", integrationEvent, integrationEvent.EventId, integrationEvent.StudentId, integrationEvent.TenantId, cancellationToken);

    public async ValueTask DisposeAsync()
    {
        await _sender.DisposeAsync();
    }

    private async Task PublishAsync<TEvent>(string subject, TEvent integrationEvent, Guid eventId, Guid studentId, Guid tenantId, CancellationToken cancellationToken)
    {
        var message = new ServiceBusMessage(JsonSerializer.Serialize(integrationEvent, SerializerOptions))
        {
            ContentType = "application/json",
            Subject = subject,
            MessageId = eventId.ToString()
        };

        message.ApplicationProperties["studentId"] = studentId.ToString();
        message.ApplicationProperties["tenantId"] = tenantId.ToString();

        await _sender.SendMessageAsync(message, cancellationToken);
        _logger.LogInformation("SERVICE BUS SEND {Subject} student:{StudentId} tenant:{TenantId}", subject, studentId, tenantId);
    }
}