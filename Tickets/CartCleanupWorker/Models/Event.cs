using System.Text.Json.Serialization;

namespace CartCleanupWorker.Models;

/// <summary>
/// Represents an event entity from Cosmos DB
/// Used to get list of events to check for expired holds
/// </summary>
public class Event
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("entityType")]
    public string EntityType { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}
