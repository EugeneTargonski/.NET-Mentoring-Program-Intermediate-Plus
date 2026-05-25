using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NotificationHandler.Services;

namespace NotificationHandler;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== Notification Handler Service ===");
        Console.WriteLine("Starting up...\n");
        Console.WriteLine("Note: Using console logging only (no database tracking)\n");

        var host = Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, config) =>
            {
                config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                config.AddEnvironmentVariables();
            })
            .ConfigureServices((context, services) =>
            {
                var configuration = context.Configuration;

                // Retrieve secrets from Azure Key Vault
                var keyVaultUri = configuration["KeyVault:VaultUri"];
                string? serviceBusConnectionString = null;
                string? mailjetApiKey = null;
                string? mailjetApiSecret = null;
                string? mailjetFromEmail = null;

                if (!string.IsNullOrWhiteSpace(keyVaultUri))
                {
                    Console.WriteLine($"Retrieving secrets from Key Vault: {keyVaultUri}");

                    try
                    {
                        var credential = new DefaultAzureCredential();
                        var secretClient = new SecretClient(new Uri(keyVaultUri), credential);

                        // Retrieve Service Bus connection string
                        var serviceBusSecret = secretClient.GetSecret("ServiceBusConnectionString");
                        serviceBusConnectionString = serviceBusSecret.Value.Value;
                        Console.WriteLine("✓ Retrieved Service Bus connection string from Key Vault");

                        // Retrieve Mailjet API key
                        var mailjetKeySecret = secretClient.GetSecret("MailjetApiKey");
                        mailjetApiKey = mailjetKeySecret.Value.Value;
                        Console.WriteLine("✓ Retrieved Mailjet API key from Key Vault");

                        // Retrieve Mailjet API secret
                        var mailjetSecretSecret = secretClient.GetSecret("MailjetApiSecret");
                        mailjetApiSecret = mailjetSecretSecret.Value.Value;
                        Console.WriteLine("✓ Retrieved Mailjet API secret from Key Vault");

                        // Retrieve Mailjet from email
                        var mailjetEmailSecret = secretClient.GetSecret("MailjetFromEmail");
                        mailjetFromEmail = mailjetEmailSecret.Value.Value;
                        Console.WriteLine("✓ Retrieved Mailjet from email from Key Vault\n");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"✗ Failed to retrieve secrets from Key Vault: {ex.Message}");
                        Console.WriteLine("Falling back to configuration or environment variables\n");
                    }
                }

                // Fall back to configuration if Key Vault retrieval failed
                serviceBusConnectionString ??= configuration["ServiceBus:ConnectionString"];
                mailjetApiKey ??= configuration["Mailjet:ApiKey"];
                mailjetApiSecret ??= configuration["Mailjet:ApiSecret"];
                mailjetFromEmail ??= configuration["Mailjet:FromEmail"];

                // Validate required secrets
                if (string.IsNullOrWhiteSpace(serviceBusConnectionString))
                {
                    throw new InvalidOperationException(
                        "Service Bus connection string not found. " +
                        "Configure it in Key Vault (ServiceBusConnectionString) or appsettings.json (ServiceBus:ConnectionString)");
                }

                if (string.IsNullOrWhiteSpace(mailjetApiKey))
                {
                    throw new InvalidOperationException(
                        "Mailjet API key not found. " +
                        "Configure it in Key Vault (MailjetApiKey) or appsettings.json (Mailjet:ApiKey)");
                }

                if (string.IsNullOrWhiteSpace(mailjetApiSecret))
                {
                    throw new InvalidOperationException(
                        "Mailjet API secret not found. " +
                        "Configure it in Key Vault (MailjetApiSecret) or appsettings.json (Mailjet:ApiSecret)");
                }

                if (string.IsNullOrWhiteSpace(mailjetFromEmail))
                {
                    throw new InvalidOperationException(
                        "Mailjet from email not found. " +
                        "Configure it in Key Vault (MailjetFromEmail) or appsettings.json (Mailjet:FromEmail)");
                }

                // Get non-sensitive configuration values
                var fromName = configuration["Mailjet:FromName"] ?? "Ticketing System";
                var queueName = configuration["ServiceBus:QueueName"] ?? "notifications";

                // Configure HttpClient for Mailjet email provider
                var mailjetCredentials = Convert.ToBase64String(
                    System.Text.Encoding.ASCII.GetBytes($"{mailjetApiKey}:{mailjetApiSecret}"));

                services.AddHttpClient<IEmailProvider, MailjetEmailProvider>()
                    .ConfigureHttpClient(client =>
                    {
                        client.BaseAddress = new Uri("https://api.mailjet.com/v3.1/");
                        client.DefaultRequestHeaders.Add("Authorization", $"Basic {mailjetCredentials}");
                    });

                // Register MailjetEmailProvider with its dependencies
                services.AddSingleton<IEmailProvider>(sp =>
                {
                    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
                    var httpClient = httpClientFactory.CreateClient(nameof(MailjetEmailProvider));
                    return new MailjetEmailProvider(httpClient, mailjetFromEmail, fromName);
                });

                // Configure Service Bus Processor
                services.AddHostedService<NotificationProcessorService>(sp =>
                {
                    var emailProvider = sp.GetRequiredService<IEmailProvider>();

                    return new NotificationProcessorService(
                        serviceBusConnectionString,
                        queueName,
                        emailProvider);
                });
            })
            .Build();

        Console.WriteLine("Notification Handler ready. Waiting for messages...\n");

        await host.RunAsync();
    }
}
