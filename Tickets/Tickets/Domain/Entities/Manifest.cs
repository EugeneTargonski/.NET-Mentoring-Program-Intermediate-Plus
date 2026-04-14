using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Tickets.Domain.Entities;

/// <summary>
/// Represents seating arrangement configuration for a venue
/// Partition Key: VenueId (manifests are queried by venue)
/// </summary>
public class Manifest : BaseEntity
{
    [JsonPropertyName("name")]
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    [MaxLength(1000)]
    public string? Description { get; set; }

    [JsonPropertyName("venueId")]
    [Required]
    public string VenueId { get; set; } = string.Empty;

    [JsonIgnore]
    public override string PartitionKey => VenueId;

    public Manifest()
    {
        EntityType = nameof(Manifest);
    }
}