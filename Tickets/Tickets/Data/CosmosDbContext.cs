using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Tickets.Data.Configuration;

namespace Tickets.Data;

/// <summary>
/// Cosmos DB Context managing 4 separate database instances
/// Based on Component Diagram architecture:
/// 1. EventDb - Event Service, Manifest Service, Offer Service
/// 2. InventoryDb - Inventory Service (Seats)
/// 3. TransactionDb - Booking Service, Payment Service
/// 4. TicketDb - Ticket Service
/// </summary>
public class CosmosDbContext(
    CosmosClient eventDbClient,
    CosmosClient inventoryDbClient,
    CosmosClient transactionDbClient,
    CosmosClient ticketDbClient,
    CosmosDbConfiguration configuration,
    ILogger<CosmosDbContext> logger) : IDisposable
{
    private readonly ILogger<CosmosDbContext> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    
    // Cosmos DB Clients (one per database instance)
    private readonly CosmosClient _eventDbClient = eventDbClient ?? throw new ArgumentNullException(nameof(eventDbClient));
    private readonly CosmosClient _inventoryDbClient = inventoryDbClient ?? throw new ArgumentNullException(nameof(inventoryDbClient));
    private readonly CosmosClient _transactionDbClient = transactionDbClient ?? throw new ArgumentNullException(nameof(transactionDbClient));
    private readonly CosmosClient _ticketDbClient = ticketDbClient ?? throw new ArgumentNullException(nameof(ticketDbClient));

    // Databases
    private Database? _eventDatabase;
    private Database? _inventoryDatabase;
    private Database? _transactionDatabase;
    private Database? _ticketDatabase;

    // Database Names
    public string EventDatabaseName { get; } = configuration.EventDb.DatabaseName;
    public string InventoryDatabaseName { get; } = configuration.InventoryDb.DatabaseName;
    public string TransactionDatabaseName { get; } = configuration.TransactionDb.DatabaseName;
    public string TicketDatabaseName { get; } = configuration.TicketDb.DatabaseName;

    // Container names by database
    // EventDb Containers
    public const string VenuesContainerName = "Venues";
    public const string EventsContainerName = "Events";
    public const string ManifestsContainerName = "Manifests";
    public const string OffersContainerName = "Offers";

    // InventoryDb Containers
    public const string SeatsContainerName = "Seats";

    // TransactionDb Containers
    public const string BookingsContainerName = "Bookings";
    public const string PaymentsContainerName = "Payments";

    // TicketDb Containers
    public const string TicketsContainerName = "Tickets";

    public async Task InitializeDatabasesAsync()
    {
        try
        {
            _logger.LogInformation("Initializing all Cosmos DB databases...");

            // Initialize EventDb
            await InitializeEventDatabaseAsync();

            // Initialize InventoryDb
            await InitializeInventoryDatabaseAsync();

            // Initialize TransactionDb
            await InitializeTransactionDatabaseAsync();

            // Initialize TicketDb
            await InitializeTicketDatabaseAsync();

            _logger.LogInformation("All Cosmos DB databases initialized successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing Cosmos DB databases");
            throw;
        }
    }

    #region EventDb Initialization

    private async Task InitializeEventDatabaseAsync()
    {
        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("Initializing EventDb: {DatabaseName}", EventDatabaseName);
        }

        _eventDatabase = await _eventDbClient.CreateDatabaseIfNotExistsAsync(EventDatabaseName);

        // Create containers for Event Service, Manifest Service, Offer Service
        await CreateContainerIfNotExistsAsync(_eventDatabase, VenuesContainerName, "/partitionKey");
        await CreateContainerIfNotExistsAsync(_eventDatabase, EventsContainerName, "/partitionKey");
        await CreateContainerIfNotExistsAsync(_eventDatabase, ManifestsContainerName, "/partitionKey");
        await CreateContainerIfNotExistsAsync(_eventDatabase, OffersContainerName, "/partitionKey");

        _logger.LogInformation("EventDb initialized successfully");
    }

    #endregion

    #region InventoryDb Initialization

    private async Task InitializeInventoryDatabaseAsync()
    {
        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("Initializing InventoryDb: {DatabaseName}", InventoryDatabaseName);
        }

        _inventoryDatabase = await _inventoryDbClient.CreateDatabaseIfNotExistsAsync(InventoryDatabaseName);

        // Create container for Inventory Service (auto-scaled for seat availability queries)
        await CreateContainerIfNotExistsAsync(
            _inventoryDatabase, 
            SeatsContainerName, 
            "/partitionKey");

        _logger.LogInformation("InventoryDb initialized successfully");
    }

    #endregion

    #region TransactionDb Initialization

    private async Task InitializeTransactionDatabaseAsync()
    {
        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("Initializing TransactionDb: {DatabaseName}", TransactionDatabaseName);
        }

        _transactionDatabase = await _transactionDbClient.CreateDatabaseIfNotExistsAsync(TransactionDatabaseName);

        // Create containers for Booking Service and Payment Service
        await CreateContainerIfNotExistsAsync(_transactionDatabase, BookingsContainerName, "/partitionKey");
        await CreateContainerIfNotExistsAsync(_transactionDatabase, PaymentsContainerName, "/partitionKey");

        _logger.LogInformation("TransactionDb initialized successfully");
    }

    #endregion

    #region TicketDb Initialization

    private async Task InitializeTicketDatabaseAsync()
    {
        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("Initializing TicketDb: {DatabaseName}", TicketDatabaseName);
        }

        _ticketDatabase = await _ticketDbClient.CreateDatabaseIfNotExistsAsync(TicketDatabaseName);

        // Create container for Ticket Service
        await CreateContainerIfNotExistsAsync(_ticketDatabase, TicketsContainerName, "/partitionKey");

        _logger.LogInformation("TicketDb initialized successfully");
    }

    #endregion

    #region Container Creation

    private async Task CreateContainerIfNotExistsAsync(
        Database database, 
        string containerName, 
        string partitionKeyPath, 
        int? throughput = null)
    {
        try
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Creating container if not exists: {DatabaseName}/{ContainerName}",
                database.Id, containerName);
            }

            var containerProperties = new ContainerProperties(containerName, partitionKeyPath)
            {
                // Add indexing policy for better query performance
                IndexingPolicy = new IndexingPolicy
                {
                    IndexingMode = IndexingMode.Consistent,
                    Automatic = true
                }
            };

            // Add specific indexes based on container
            ConfigureIndexingPolicy(containerName, containerProperties.IndexingPolicy);

            Container container;
            if (throughput.HasValue)
            {
                container = await database.CreateContainerIfNotExistsAsync(containerProperties, throughput.Value);
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Container created with {Throughput} RU/s: {ContainerName}", throughput.Value, containerName);
                }
            }
            else
            {
                container = await database.CreateContainerIfNotExistsAsync(containerProperties);
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Container created: {ContainerName}", containerName);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating container: {DatabaseName}/{ContainerName}", database.Id, containerName);
            throw;
        }
    }

    private static void ConfigureIndexingPolicy(string containerName, IndexingPolicy indexingPolicy)
    {
        // Always include the root path for basic indexing
        indexingPolicy.IncludedPaths.Add(new IncludedPath { Path = "/*" });

        switch (containerName)
        {
            case SeatsContainerName:
                // Inventory Service - Critical indexes for seat queries
                indexingPolicy.IncludedPaths.Add(new IncludedPath { Path = "/status/?" });
                indexingPolicy.IncludedPaths.Add(new IncludedPath { Path = "/holdExpiresAt/?" });
                indexingPolicy.IncludedPaths.Add(new IncludedPath { Path = "/currentOffer/price/?" });
                indexingPolicy.IncludedPaths.Add(new IncludedPath { Path = "/section/?" });
                indexingPolicy.IncludedPaths.Add(new IncludedPath { Path = "/row/?" });
                break;

            case EventsContainerName:
                // Event Service - Indexes for event queries
                indexingPolicy.IncludedPaths.Add(new IncludedPath { Path = "/eventDate/?" });
                indexingPolicy.IncludedPaths.Add(new IncludedPath { Path = "/isActive/?" });
                indexingPolicy.IncludedPaths.Add(new IncludedPath { Path = "/category/?" });
                break;

            case BookingsContainerName:
                // Booking Service - Indexes for booking queries
                indexingPolicy.IncludedPaths.Add(new IncludedPath { Path = "/status/?" });
                indexingPolicy.IncludedPaths.Add(new IncludedPath { Path = "/eventId/?" });
                indexingPolicy.IncludedPaths.Add(new IncludedPath { Path = "/createdAt/?" });
                break;

            case PaymentsContainerName:
                // Payment Service - Indexes for payment queries
                indexingPolicy.IncludedPaths.Add(new IncludedPath { Path = "/status/?" });
                indexingPolicy.IncludedPaths.Add(new IncludedPath { Path = "/transactionId/?" });
                indexingPolicy.IncludedPaths.Add(new IncludedPath { Path = "/bookingId/?" });
                break;

            case TicketsContainerName:
                // Ticket Service - Indexes for ticket queries
                indexingPolicy.IncludedPaths.Add(new IncludedPath { Path = "/ticketNumber/?" });
                indexingPolicy.IncludedPaths.Add(new IncludedPath { Path = "/bookingId/?" });
                indexingPolicy.IncludedPaths.Add(new IncludedPath { Path = "/isUsed/?" });
                break;

            case OffersContainerName:
                // Offer Service - Indexes for offer queries
                indexingPolicy.IncludedPaths.Add(new IncludedPath { Path = "/priceCategory/?" });
                indexingPolicy.IncludedPaths.Add(new IncludedPath { Path = "/isActive/?" });
                indexingPolicy.IncludedPaths.Add(new IncludedPath { Path = "/price/?" });
                break;
        }
    }

    #endregion

    #region Container Getters

    /// <summary>
    /// Get container from EventDb (Event Service, Manifest Service, Offer Service)
    /// </summary>
    public Container GetEventDbContainer(string containerName)
    {
        if (_eventDatabase == null)
            throw new InvalidOperationException("EventDb not initialized. Call InitializeDatabasesAsync first.");

        return _eventDatabase.GetContainer(containerName);
    }

    /// <summary>
    /// Get container from InventoryDb (Inventory Service)
    /// </summary>
    public Container GetInventoryDbContainer(string containerName)
    {
        if (_inventoryDatabase == null)
            throw new InvalidOperationException("InventoryDb not initialized. Call InitializeDatabasesAsync first.");

        return _inventoryDatabase.GetContainer(containerName);
    }

    /// <summary>
    /// Get container from TransactionDb (Booking Service, Payment Service)
    /// </summary>
    public Container GetTransactionDbContainer(string containerName)
    {
        if (_transactionDatabase == null)
            throw new InvalidOperationException("TransactionDb not initialized. Call InitializeDatabasesAsync first.");

        return _transactionDatabase.GetContainer(containerName);
    }

    /// <summary>
    /// Get container from TicketDb (Ticket Service)
    /// </summary>
    public Container GetTicketDbContainer(string containerName)
    {
        if (_ticketDatabase == null)
            throw new InvalidOperationException("TicketDb not initialized. Call InitializeDatabasesAsync first.");

        return _ticketDatabase.GetContainer(containerName);
    }

    #endregion

    public void Dispose()
    {
        _eventDbClient?.Dispose();
        _inventoryDbClient?.Dispose();
        _transactionDbClient?.Dispose();
        _ticketDbClient?.Dispose();
        GC.SuppressFinalize(this);
    }
}