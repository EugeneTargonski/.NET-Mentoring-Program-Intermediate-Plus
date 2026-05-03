using Azure.Identity;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.DependencyInjection;
using Tickets.Infrastructure;
using Azure.Core.Diagnostics;
using System.Diagnostics.Tracing;

namespace Tickets;

/// <summary>
/// Application entry point - Responsibility: Application lifecycle management
/// </summary>
public class Program
{
    public static async Task Main(string[] args)
    {
        // Enable Azure SDK logging for credential troubleshooting
        using var listener = AzureEventSourceListener.CreateConsoleLogger(EventLevel.Verbose);

        var builder = WebApplication.CreateBuilder(args);

        // Configure Azure App Configuration for centralized configuration management
        ConfigureAzureAppConfiguration(builder);

        // Add services to the container
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        // Register global exception handler
        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
        builder.Services.AddProblemDetails();

        // Configure DI for DAL
        DependencyInjectionConfiguration.ConfigureServices(
            builder.Services,
            builder.Configuration);

        // Add Azure App Configuration refresh support (not in Testing environment)
        if (builder.Environment.EnvironmentName != "Testing")
        {
            builder.Services.AddAzureAppConfiguration();
        }

        var app = builder.Build();

        // Use global exception handler
        app.UseExceptionHandler();

        // Use Azure App Configuration middleware for dynamic refresh (not in Testing environment)
        if (!app.Environment.IsEnvironment("Testing"))
        {
            app.UseAzureAppConfiguration();
        }

        // Configure the HTTP request pipeline
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.UseAuthorization();
        app.MapControllers();

        await app.RunAsync();
    }

    private static void ConfigureAzureAppConfiguration(WebApplicationBuilder builder)
    {
        var appConfigSettings = new AzureAppConfigurationSettings();
        builder.Configuration.GetSection("AzureAppConfiguration").Bind(appConfigSettings);

        // Only configure App Configuration if enabled
        if (!appConfigSettings.Enabled)
        {
            return;
        }

        // If Key Vault URI is specified, retrieve connection string from Key Vault
        if (!string.IsNullOrWhiteSpace(appConfigSettings.KeyVaultUri) && 
            !string.IsNullOrWhiteSpace(appConfigSettings.ConnectionStringSecretName))
        {
            try
            {
                Console.WriteLine($"Retrieving App Configuration connection string from Key Vault: {appConfigSettings.KeyVaultUri}");

                var keyVaultUri = new Uri(appConfigSettings.KeyVaultUri);
                var credential = new DefaultAzureCredential();
                var secretClient = new Azure.Security.KeyVault.Secrets.SecretClient(keyVaultUri, credential);

                var secret = secretClient.GetSecret(appConfigSettings.ConnectionStringSecretName);
                appConfigSettings.ConnectionString = secret.Value.Value;

                Console.WriteLine("Successfully retrieved connection string from Key Vault");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: Failed to retrieve connection string from Key Vault");
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        // Determine connection method: Connection String (dev) or Endpoint (prod with Managed Identity)
        var hasConnectionString = !string.IsNullOrWhiteSpace(appConfigSettings.ConnectionString);
        var hasEndpoint = !string.IsNullOrWhiteSpace(appConfigSettings.Endpoint);

        if (!hasConnectionString && !hasEndpoint)
        {
            // No App Configuration configured, skip
            return;
        }

        try
        {
            builder.Configuration.AddAzureAppConfiguration(options =>
            {
                // Connect using Connection String (local dev) or Endpoint (production with Managed Identity)
                if (hasConnectionString)
                {
                    options.Connect(appConfigSettings.ConnectionString);
                }


                // Select configuration keys with optional label filter
                if (!string.IsNullOrWhiteSpace(appConfigSettings.Label))
                {
                    // Load configuration with specific label (e.g., "Development", "Production")
                    options.Select(KeyFilter.Any, appConfigSettings.Label);
                }
                else
                {
                    // Load all configuration without label filter
                    options.Select(KeyFilter.Any, LabelFilter.Null);
                }

                // Configure dynamic refresh with sentinel key
                if (!string.IsNullOrWhiteSpace(appConfigSettings.SentinelKey))
                {
                    options.ConfigureRefresh(refresh =>
                    {
                        // Watch sentinel key - when it changes, refresh all configuration
                        refresh.Register(appConfigSettings.SentinelKey, refreshAll: true)
                               .SetRefreshInterval(TimeSpan.FromSeconds(appConfigSettings.CacheExpirationSeconds));
                    });
                }

                // Configure Key Vault integration for secrets
                if (appConfigSettings.UseKeyVaultReferences)
                {
                    // Use simple DefaultAzureCredential like Key Vault (no explicit options)
                    var credential = new DefaultAzureCredential();

                    // Automatically resolve Key Vault references in App Configuration
                    options.ConfigureKeyVault(kv =>
                    {
                        kv.SetCredential(credential);
                    });
                }
            });
        }
        catch (Exception ex)
        {
            // Log detailed error information for troubleshooting
            Console.WriteLine($"ERROR: Failed to connect to Azure App Configuration");
            Console.WriteLine($"Endpoint: {appConfigSettings.Endpoint}");
            Console.WriteLine($"Error Type: {ex.GetType().Name}");
            Console.WriteLine($"Error Message: {ex.Message}");

            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
            }

            Console.WriteLine("\nTroubleshooting steps:");
            Console.WriteLine("1. Verify you're signed in to Visual Studio (Tools → Options → Azure Service Authentication)");
            Console.WriteLine("2. Ensure your account has 'App Configuration Data Reader' role on the resource");
            Console.WriteLine("3. Try running 'az login' in the terminal");
            Console.WriteLine("4. Check if you need to specify TenantId in appsettings");

            throw; // Re-throw to stop the application since configuration is required
        }
    }
}
