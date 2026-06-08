using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using CartCleanupWorker;
using CartCleanupWorker.Configuration;
using CartCleanupWorker.Services;
using Microsoft.Azure.Cosmos;

var builder = Host.CreateApplicationBuilder(args);

// Configure settings
var keyVaultSettings = new KeyVaultSettings();
builder.Configuration.GetSection("KeyVault").Bind(keyVaultSettings);

var cosmosDbSettings = new CosmosDbSettings();
builder.Configuration.GetSection("CosmosDb").Bind(cosmosDbSettings);

var cartCleanupSettings = new CartCleanupSettings();
builder.Configuration.GetSection("CartCleanup").Bind(cartCleanupSettings);

// Retrieve Cosmos DB connection string from Key Vault if configured
if (!string.IsNullOrWhiteSpace(keyVaultSettings.VaultUri))
{
    try
    {
        Console.WriteLine($"Retrieving secrets from Key Vault: {keyVaultSettings.VaultUri}");

        var credential = new DefaultAzureCredential();
        var secretClient = new SecretClient(new Uri(keyVaultSettings.VaultUri), credential);

        // Retrieve Cosmos DB connection string
        var cosmosSecret = secretClient.GetSecret(cosmosDbSettings.ConnectionStringSecretName);
        cosmosDbSettings.ConnectionString = cosmosSecret.Value.Value;

        Console.WriteLine("✓ Retrieved Cosmos DB connection string from Key Vault");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"✗ Failed to retrieve secrets from Key Vault: {ex.Message}");
        Console.WriteLine("Falling back to configuration");
    }
}

// Fall back to configuration if Key Vault retrieval failed
if (string.IsNullOrWhiteSpace(cosmosDbSettings.ConnectionString))
{
    cosmosDbSettings.ConnectionString = builder.Configuration["CosmosDb:ConnectionString"] ?? string.Empty;
}

// Validate Cosmos DB connection string
if (string.IsNullOrWhiteSpace(cosmosDbSettings.ConnectionString))
{
    throw new InvalidOperationException(
        "Cosmos DB connection string not found. " +
        "Configure it in Key Vault or appsettings.json (CosmosDb:ConnectionString)");
}

// Register Cosmos Client as singleton
builder.Services.AddSingleton<CosmosClient>(sp =>
{
    var cosmosClientOptions = new CosmosClientOptions
    {
        SerializerOptions = new CosmosSerializationOptions
        {
            PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
        }
    };

    return new CosmosClient(cosmosDbSettings.ConnectionString, cosmosClientOptions);
});

// Register settings
builder.Services.AddSingleton(cosmosDbSettings);
builder.Services.Configure<CartCleanupSettings>(builder.Configuration.GetSection("CartCleanup"));

// Register services
builder.Services.AddSingleton<ICartCleanupService, CartCleanupService>();

// Register the Worker
builder.Services.AddHostedService<Worker>();

var host = builder.Build();

Console.WriteLine("=== Cart Cleanup Worker Service ===");
Console.WriteLine($"Cleanup interval: {cartCleanupSettings.IntervalSeconds} seconds");
Console.WriteLine($"Cart expiration: {cartCleanupSettings.CartExpirationMinutes} minutes");
Console.WriteLine($"Cosmos DB: {cosmosDbSettings.DatabaseName}/{cosmosDbSettings.ContainerName}");
Console.WriteLine("Starting worker...\n");

await host.RunAsync();
