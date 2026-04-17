namespace Tickets.Data.Configuration;

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