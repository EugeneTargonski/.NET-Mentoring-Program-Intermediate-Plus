using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Tickets.Data.Abstractions;
using Tickets.Data.Configuration;
using Tickets.Data.UnitOfWork;
using Tickets.Demo;

namespace Tickets.Infrastructure;

/// <summary>
/// Responsibility: Configure dependency injection container
/// </summary>
public static class DependencyInjectionConfiguration
{
    public static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // Logging is configured in Program.cs via Host.CreateDefaultBuilder
        // No need to configure it here

        // Add Cosmos DB configuration
        var cosmosDbConfig = new CosmosDbConfiguration();
        configuration.GetSection("CosmosDb").Bind(cosmosDbConfig);
        services.AddSingleton(cosmosDbConfig);

        // Register Cosmos DB infrastructure
        CosmosDbServiceRegistration.RegisterCosmosDbServices(services, cosmosDbConfig);

        // Register demo orchestrator
        services.AddTransient<DemoOrchestrator>();
        
        // Register all demo scenarios
        RegisterDemoScenarios(services);

        // Register abstraction → implementation mapping
        services.AddScoped<IUnitOfWork, CosmosUnitOfWork>();  // ✅ Easy to swap implementations
    }

    private static void RegisterDemoScenarios(IServiceCollection services)
    {
        services.AddTransient<EventDemoScenarios>();
        services.AddTransient<InventoryDemoScenarios>();
        services.AddTransient<BookingDemoScenarios>();
        services.AddTransient<PaymentDemoScenarios>();
        services.AddTransient<TicketDemoScenarios>();
    }
}