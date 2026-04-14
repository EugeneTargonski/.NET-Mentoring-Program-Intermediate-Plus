using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Tickets.Data;
using Tickets.Data.Abstractions;
using Tickets.Data.Configuration;
using Tickets.Data.UnitOfWork;

namespace Tickets.Infrastructure;

/// <summary>
/// Responsibility: Register Cosmos DB services and clients
/// </summary>
public static class CosmosDbServiceRegistration
{
    public static void RegisterCosmosDbServices(
        IServiceCollection services, 
        CosmosDbConfiguration configuration)
    {
        // Create and register Cosmos DB clients for each database
        var eventDbClient = CreateCosmosClient(configuration.EventDb);
        var inventoryDbClient = CreateCosmosClient(configuration.InventoryDb);
        var transactionDbClient = CreateCosmosClient(configuration.TransactionDb);
        var ticketDbClient = CreateCosmosClient(configuration.TicketDb);

        // Register clients as singletons
        services.AddSingleton(_ => eventDbClient);
        services.AddSingleton(_ => inventoryDbClient);
        services.AddSingleton(_ => transactionDbClient);
        services.AddSingleton(_ => ticketDbClient);

        // Register CosmosDbContext
        services.AddSingleton(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<CosmosDbContext>>();
            var config = sp.GetRequiredService<CosmosDbConfiguration>();
            
            var context = new CosmosDbContext(
                eventDbClient,
                inventoryDbClient,
                transactionDbClient,
                ticketDbClient,
                config,
                logger);

            return context;
        });

        // Register database initializer
        services.AddSingleton<IDatabaseInitializer, DatabaseInitializer>();

        // Register Unit of Work (database-agnostic abstraction)
        services.AddScoped<IUnitOfWork, CosmosUnitOfWork>();
    }

    private static CosmosClient CreateCosmosClient(CosmosDbSettings settings)
    {
        var cosmosClientOptions = new CosmosClientOptions
        {
            ApplicationName = settings.ApplicationName,
            AllowBulkExecution = settings.AllowBulkExecution,
            MaxRetryAttemptsOnRateLimitedRequests = settings.MaxRetryAttemptsOnRateLimitedRequests,
            MaxRetryWaitTimeOnRateLimitedRequests = TimeSpan.FromSeconds(settings.MaxRetryWaitTimeOnRateLimitedRequests),
            SerializerOptions = new CosmosSerializationOptions
            {
                PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
            }
        };

        return new CosmosClient(settings.EndpointUri, settings.PrimaryKey, cosmosClientOptions);
    }
}