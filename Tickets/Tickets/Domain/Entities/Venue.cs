using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Tickets.Domain.Entities;

/// <summary>
/// Represents a physical location where events take place
/// Partition Key: Id (for independent venue queries)
/// </summary>
public class Venue : BaseEntity
{
    [JsonPropertyName("name")]
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("address")]
    [Required]
    [MaxLength(500)]
    public string Address { get; set; } = string.Empty;

    [JsonPropertyName("city")]
    [MaxLength(100)]
    public string? City { get; set; }

    [JsonPropertyName("country")]
    [MaxLength(100)]
    public string? Country { get; set; }

    [JsonPropertyName("capacity")]
    [Range(1, 500000)]
    public int Capacity { get; set; }

    [JsonIgnore]
    public override string PartitionKey => Id;

    public Venue()
    {
        EntityType = nameof(Venue);
    }
}