using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Tickets.Domain.Enums;

namespace Tickets.Domain.Entities;

/// <summary>
/// Represents pricing configuration for seats
/// Partition Key: Id (global offers)
/// </summary>
public class Offer : BaseEntity
{
    [JsonPropertyName("name")]
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    [MaxLength(1000)]
    public string? Description { get; set; }

    [JsonPropertyName("price")]
    [Required]
    [Range(0.01, 1000000)]
    public decimal Price { get; set; }

    [JsonPropertyName("priceCategory")]
    [Required]
    public PriceCategory PriceCategory { get; set; }

    [JsonPropertyName("validFrom")]
    public DateTime? ValidFrom { get; set; }

    [JsonPropertyName("validTo")]
    public DateTime? ValidTo { get; set; }

    [JsonPropertyName("isActive")]
    public bool IsActive { get; set; } = true;

    [JsonIgnore]
    public override string PartitionKey => Id;

    public Offer()
    {
        EntityType = nameof(Offer);
    }
}