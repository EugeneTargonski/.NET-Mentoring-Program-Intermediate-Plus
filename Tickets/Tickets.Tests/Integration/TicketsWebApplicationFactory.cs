using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Tickets.Data;
using Tickets.Data.Abstractions;
using Tickets.Infrastructure;
using Tickets.Services.Abstractions;
using Tickets.Tests.Mocks;

namespace Tickets.Tests.Integration;

/// <summary>
/// Custom WebApplicationFactory for integration testing between API and DAL layers
/// Uses mock repositories instead of real Cosmos DB
/// </summary>
public class TicketsWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove Cosmos DB related services
            RemoveCosmosDbServices(services);

            // Register mock UnitOfWork for testing
            services.AddScoped<IUnitOfWork>(_ => MockDataAccessProvider.CreateMockUnitOfWork());
        });

        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Add in-memory configuration for testing
            var testConfiguration = new Dictionary<string, string>
            {
                // Disable Azure App Configuration for testing
                ["AzureAppConfiguration:Enabled"] = "false"
            };

            config.AddInMemoryCollection(testConfiguration!);
        });

        builder.UseEnvironment("Testing");
    }

    private static void RemoveCosmosDbServices(IServiceCollection services)
    {
        // Remove Cosmos DB Context and related infrastructure
        var cosmosDbContextDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(CosmosDbContext));
        if (cosmosDbContextDescriptor != null)
        {
            services.Remove(cosmosDbContextDescriptor);
        }

        // Remove existing UnitOfWork registration
        var unitOfWorkDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IUnitOfWork));
        if (unitOfWorkDescriptor != null)
        {
            services.Remove(unitOfWorkDescriptor);
        }

        // Remove DatabaseInitializer
        var dbInitializerDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IDatabaseInitializer));
        if (dbInitializerDescriptor != null)
        {
            services.Remove(dbInitializerDescriptor);
        }

        // Remove CosmosClient registrations
        var cosmosClientDescriptors = services.Where(d => d.ServiceType.Name.Contains("CosmosClient")).ToList();
        foreach (var descriptor in cosmosClientDescriptors)
        {
            services.Remove(descriptor);
        }
    }
}

