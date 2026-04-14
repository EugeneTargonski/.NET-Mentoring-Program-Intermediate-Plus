using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Tickets.Domain.Entities;

/// <summary>
/// Base entity for Cosmos DB documents
/// </summary>
public abstract class BaseEntity
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonPropertyName("partitionKey")]
    public abstract string PartitionKey { get; }

    [JsonPropertyName("entityType")]
    public string EntityType { get; set; } = string.Empty;

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [JsonPropertyName("updatedAt")]
    public DateTime? UpdatedAt { get; set; }

    [JsonPropertyName("_etag")]
    public string? ETag { get; set; }
}