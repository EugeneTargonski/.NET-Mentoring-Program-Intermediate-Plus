using Microsoft.Extensions.Caching.Memory;
using Tickets.Data.Abstractions;
using Tickets.Data.Configuration;
using Tickets.Data.UnitOfWork;
using Tickets.Services;
using Tickets.Services.Abstractions;
using Tickets.Services.Infrastructure;

namespace Tickets.Infrastructure;

/// <summary>
/// SOLID-compliant Dependency Injection Configuration
/// </summary>
public static class DependencyInjectionConfiguration
{
    public static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // Add Memory Cache
        services.AddMemoryCache();

        // Add Cosmos DB configuration
        var cosmosDbConfig = new CosmosDbConfiguration();
        configuration.GetSection("CosmosDb").Bind(cosmosDbConfig);
        services.AddSingleton(cosmosDbConfig);

        // Register Cosmos DB infrastructure
        CosmosDbServiceRegistration.RegisterCosmosDbServices(services, cosmosDbConfig);

        // Register abstraction → implementation mapping
        services.AddScoped<IUnitOfWork, CosmosUnitOfWork>();

        // Infrastructure services (DIP compliance)
        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
        services.AddSingleton<ICartStorageProvider, InMemoryCartStorageProvider>();

        // Domain services (SRP compliance)
        services.AddScoped<ISeatService, SeatService>();
        services.AddScoped<IBookingService, BookingService>();

        // API services
        services.AddScoped<IVenueService, VenueService>();

        // Event service with decorator pattern (OCP compliance)
        services.AddScoped<EventService>(); // Core implementation
        services.AddScoped<IEventService>(sp => 
            new CachedEventService(
                sp.GetRequiredService<EventService>(), 
                sp.GetRequiredService<IMemoryCache>()));

        // Cache management service (SRP compliance)
        services.AddScoped<IEventCacheService, EventCacheService>();

        // Use refactored services (SOLID-compliant)
        services.AddScoped<ICartService, CartService>();
        services.AddScoped<IPaymentService, PaymentService>();
    }

}
