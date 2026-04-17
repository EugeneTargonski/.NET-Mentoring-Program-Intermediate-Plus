namespace Tickets.Data.Configuration
{
    /// <summary>
    /// Cosmos DB configuration settings for a single database instance
    /// </summary>
    public class CosmosDbSettings
    {
        public string EndpointUri { get; set; } = string.Empty;
        public string PrimaryKey { get; set; } = string.Empty;
        public string DatabaseName { get; set; } = string.Empty;
        public string ApplicationName { get; set; } = string.Empty;
        public bool AllowBulkExecution { get; set; } = true;
        public int MaxRetryAttemptsOnRateLimitedRequests { get; set; } = 5;
        public int MaxRetryWaitTimeOnRateLimitedRequests { get; set; } = 30;
    }
}
