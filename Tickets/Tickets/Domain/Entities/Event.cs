using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Tickets.Domain.Entities;

/// <summary>
/// Represents an event (concert, sports game, etc.)
/// Partition Key: Id (for event-specific queries)
/// </summary>
public class Event : BaseEntity
{
    [JsonPropertyName("name")]
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    [MaxLength(2000)]
    public string? Description { get; set; }

    [JsonPropertyName("eventDate")]
    [Required]
    public DateTime EventDate { get; set; }

    [JsonPropertyName("eventEndDate")]
    public DateTime? EventEndDate { get; set; }

    [JsonPropertyName("venueId")]
    [Required]
    public string VenueId { get; set; } = string.Empty;

    [JsonPropertyName("manifestId")]
    [Required]
    public string ManifestId { get; set; } = string.Empty;

    [JsonPropertyName("category")]
    [MaxLength(100)]
    public string? Category { get; set; }

    [JsonPropertyName("isActive")]
    public bool IsActive { get; set; } = true;

    // Embedded venue information (denormalized for performance)
    [JsonPropertyName("venue")]
    public VenueInfo? Venue { get; set; }

    [JsonIgnore]
    public override string PartitionKey => Id;

    public Event()
    {
        EntityType = nameof(Event);
    }
}