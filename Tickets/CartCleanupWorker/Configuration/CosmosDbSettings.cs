namespace CartCleanupWorker.Configuration;

/// <summary>
/// Settings for Cosmos DB configuration
/// </summary>
public class CosmosDbSettings
{
    public string ConnectionStringSecretName { get; set; } = "CosmosDbConnectionString";
    public string DatabaseName { get; set; } = "TicketingDB";
    public string ContainerName { get; set; } = "Items";
    public string ConnectionString { get; set; } = string.Empty;
}
