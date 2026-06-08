using System.Text.Json.Serialization;

namespace CartCleanupWorker.Models;

/// <summary>
/// Represents a seat entity from Cosmos DB
/// Matches the structure from Tickets.Domain.Entities.Seat
/// </summary>
public class Seat
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("entityType")]
    public string EntityType { get; set; } = string.Empty;

    [JsonPropertyName("eventId")]
    public string EventId { get; set; } = string.Empty;

    [JsonPropertyName("seatNumber")]
    public string SeatNumber { get; set; } = string.Empty;

    [JsonPropertyName("row")]
    public string? Row { get; set; }

    [JsonPropertyName("section")]
    public string? Section { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = "Available";

    [JsonPropertyName("holdExpiresAt")]
    public DateTime? HoldExpiresAt { get; set; }

    [JsonPropertyName("heldByCustomerId")]
    public string? HeldByCustomerId { get; set; }

    [JsonPropertyName("_etag")]
    public string? ETag { get; set; }
}
