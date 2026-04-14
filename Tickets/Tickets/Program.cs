using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Tickets.Infrastructure;
using Tickets.Demo;

namespace Tickets;

/// <summary>
/// Application entry point - Responsibility: Application lifecycle management
/// </summary>
internal class Program
{
    static async Task Main(string[] args)
    {
        try
        {
            // Build and run the application
            var host = CreateHostBuilder(args).Build();
            
            var logger = host.Services.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("Starting Tickets Data Access Layer Demo (4 Cosmos DB Instances)");

            // Run demo scenarios
            await host.Services.GetRequiredService<DemoOrchestrator>().RunAllDemosAsync();

            logger.LogInformation("Demo completed successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Application terminated unexpectedly: {ex}");
            throw;
        }
    }

    private static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureLogging((hostContext, logging) =>
            {
                logging.ClearProviders();
                logging.AddConfiguration(hostContext.Configuration.GetSection("Logging"));
                logging.AddConsole();
                logging.AddDebug();
            })
            .ConfigureServices((hostContext, services) =>
            {
                // Delegate DI configuration to separate class
                DependencyInjectionConfiguration.ConfigureServices(
                    services, 
                    hostContext.Configuration);
            });
}
