namespace Tickets.Data.Configuration;

/// <summary>
/// Configuration for Azure Service Bus
/// </summary>
public class ServiceBusSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public string QueueName { get; set; } = "notifications";
}
