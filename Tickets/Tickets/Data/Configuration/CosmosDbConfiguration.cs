namespace Tickets.Data.Configuration;

/// <summary>
/// Cosmos DB configuration settings for a single database instance
/// </summary>
public class CosmosDbSettings
{
    public string EndpointUri { get; set; } = string.Empty;
    public string PrimaryKey { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
    public string ApplicationName { get; set; } = string.Empty;
    public bool AllowBulkExecution { get; set; } = true;
    public int MaxRetryAttemptsOnRateLimitedRequests { get; set; } = 5;
    public int MaxRetryWaitTimeOnRateLimitedRequests { get; set; } = 30;
}

/// <summary>
/// Root configuration for all 4 Cosmos DB instances
/// Based on Component Diagram:
/// - EventDb: Events, Venues, Manifests, Offers
/// - InventoryDb: Seats (Inventory Service)
/// - TransactionDb: Bookings, Payments (Booking & Payment Services)
/// - TicketDb: Tickets (Ticket Service)
/// </summary>
public class CosmosDbConfiguration
{
    public CosmosDbSettings EventDb { get; set; } = new();
    public CosmosDbSettings InventoryDb { get; set; } = new();
    public CosmosDbSettings TransactionDb { get; set; } = new();
    public CosmosDbSettings TicketDb { get; set; } = new();
}