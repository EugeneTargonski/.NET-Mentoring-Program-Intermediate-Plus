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
        // Register typed CosmosClient wrappers as singletons
        // The DI container will automatically dispose them on application shutdown
        services.AddSingleton(sp => 
            new EventDbCosmosClient(CreateCosmosClient(configuration.EventDb)));

        services.AddSingleton(sp => 
            new InventoryDbCosmosClient(CreateCosmosClient(configuration.InventoryDb)));

        services.AddSingleton(sp => 
            new TransactionDbCosmosClient(CreateCosmosClient(configuration.TransactionDb)));

        services.AddSingleton(sp => 
            new TicketDbCosmosClient(CreateCosmosClient(configuration.TicketDb)));

        // Register CosmosDbContext with explicit typed client resolution
        services.AddSingleton<CosmosDbContext>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<CosmosDbContext>>();
            var config = sp.GetRequiredService<CosmosDbConfiguration>();

            // Resolve typed clients - type-safe and explicit
            var eventDbClient = sp.GetRequiredService<EventDbCosmosClient>().Client;
            var inventoryDbClient = sp.GetRequiredService<InventoryDbCosmosClient>().Client;
            var transactionDbClient = sp.GetRequiredService<TransactionDbCosmosClient>().Client;
            var ticketDbClient = sp.GetRequiredService<TicketDbCosmosClient>().Client;

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