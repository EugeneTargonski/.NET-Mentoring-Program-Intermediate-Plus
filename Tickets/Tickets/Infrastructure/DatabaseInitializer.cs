using Microsoft.Extensions.Logging;
using Tickets.Data;
using Tickets.Data.Configuration;

namespace Tickets.Infrastructure;

/// <summary>
/// Responsibility: Initialize all 4 Cosmos DB database instances
/// </summary>
public class DatabaseInitializer(
    CosmosDbContext context,
    CosmosDbConfiguration configuration,
    ILogger<DatabaseInitializer> logger) : IDatabaseInitializer
{
    private readonly CosmosDbContext _context = context ?? throw new ArgumentNullException(nameof(context));
    private readonly CosmosDbConfiguration _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    private readonly ILogger<DatabaseInitializer> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task InitializeAsync()
    {
        _logger.LogInformation("Initializing all Cosmos DB databases...");

        await _context.InitializeDatabasesAsync();
        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("All 4 Cosmos DB instances initialized:");
            _logger.LogInformation("  - EventDb: {DatabaseName}", _configuration.EventDb.DatabaseName);
            _logger.LogInformation("  - InventoryDb: {DatabaseName}", _configuration.InventoryDb.DatabaseName);
            _logger.LogInformation("  - TransactionDb: {DatabaseName}", _configuration.TransactionDb.DatabaseName);
            _logger.LogInformation("  - TicketDb: {DatabaseName}", _configuration.TicketDb.DatabaseName);
        }
    }
}