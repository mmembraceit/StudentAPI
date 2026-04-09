namespace StudentApi.Infrastructure.Messaging;

/// <summary>
/// Configuration for Azure Service Bus integration.
/// </summary>
public sealed class AzureServiceBusOptions
{
    public const string SectionName = "AzureServiceBus";

    /// <summary>
    /// Connection string used by the Azure Service Bus client.
    /// </summary>
    public string ConnectionString { get; init; } = string.Empty;

    /// <summary>
    /// Queue name that receives student integration events.
    /// </summary>
    public string QueueName { get; init; } = string.Empty;
}