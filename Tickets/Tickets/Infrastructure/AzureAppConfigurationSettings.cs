namespace Tickets.Infrastructure;

/// <summary>
/// Configuration for Azure App Configuration integration
/// </summary>
public class AzureAppConfigurationSettings
{
    /// <summary>
    /// Azure App Configuration connection string or endpoint
    /// Use connection string for local dev, endpoint for Managed Identity in production
    /// </summary>
    public string? Endpoint { get; set; }

    /// <summary>
    /// Connection string for Azure App Configuration (for local development)
    /// </summary>
    public string? ConnectionString { get; set; }

    /// <summary>
    /// Whether to use Azure App Configuration
    /// Set to false for local development without App Configuration
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Label filter for configuration (e.g., "Development", "Production")
    /// Allows multiple environments in same App Configuration store
    /// </summary>
    public string? Label { get; set; }

    /// <summary>
    /// Cache expiration time in seconds for configuration refresh
    /// Default: 30 seconds
    /// </summary>
    public int CacheExpirationSeconds { get; set; } = 30;

    /// <summary>
    /// Whether to use Key Vault references in App Configuration
    /// When true, App Configuration will resolve Key Vault references automatically
    /// </summary>
    public bool UseKeyVaultReferences { get; set; } = true;

    /// <summary>
    /// Tenant ID for Azure AD authentication (optional)
    /// </summary>
    public string? TenantId { get; set; }

    /// <summary>
    /// Sentinel key to watch for configuration refresh
    /// When this key changes, all configuration is reloaded
    /// </summary>
    public string? SentinelKey { get; set; } = "Sentinel";

    /// <summary>
    /// Key Vault URI where the App Configuration connection string is stored
    /// When specified, retrieves the connection string from Key Vault secret
    /// </summary>
    public string? KeyVaultUri { get; set; }

    /// <summary>
    /// Name of the Key Vault secret containing the App Configuration connection string
    /// Default: "AppConfigConnectionString"
    /// </summary>
    public string? ConnectionStringSecretName { get; set; } = "AppConfigConnectionString";
}
