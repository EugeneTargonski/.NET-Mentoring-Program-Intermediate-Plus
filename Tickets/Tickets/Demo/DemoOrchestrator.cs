using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Tickets.Infrastructure;

namespace Tickets.Demo;

/// <summary>
/// Responsibility: Orchestrate demo scenario execution
/// </summary>
public class DemoOrchestrator(
    IServiceProvider serviceProvider,
    IDatabaseInitializer databaseInitializer,
    ILogger<DemoOrchestrator> logger)
{
    private readonly IServiceProvider _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    private readonly IDatabaseInitializer _databaseInitializer = databaseInitializer ?? throw new ArgumentNullException(nameof(databaseInitializer));
    private readonly ILogger<DemoOrchestrator> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task RunAllDemosAsync()
    {
        // Initialize databases first
        await _databaseInitializer.InitializeAsync();

        using var scope = _serviceProvider.CreateScope();

        _logger.LogInformation("=== Starting CRUD Operations Demo (4 Cosmos DB Instances) ===");

        try
        {
            // Run Event Service demos
            var eventDemos = scope.ServiceProvider.GetRequiredService<EventDemoScenarios>();
            await eventDemos.RunAllAsync();

            // Run Inventory Service demos
            var inventoryDemos = scope.ServiceProvider.GetRequiredService<InventoryDemoScenarios>();
            await inventoryDemos.RunAllAsync();

            // Run Booking Service demos
            var bookingDemos = scope.ServiceProvider.GetRequiredService<BookingDemoScenarios>();
            await bookingDemos.RunAllAsync();

            // Run Payment Service demos
            var paymentDemos = scope.ServiceProvider.GetRequiredService<PaymentDemoScenarios>();
            await paymentDemos.RunAllAsync();

            // Run Ticket Service demos
            var ticketDemos = scope.ServiceProvider.GetRequiredService<TicketDemoScenarios>();
            await ticketDemos.RunAllAsync();

            _logger.LogInformation("=== All Demos Completed Successfully ===");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running demos");
            throw;
        }
    }
}