using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Tickets.Domain.Enums;

namespace Tickets.Domain.Entities;

/// <summary>
/// Represents an individual seat with its availability status
/// Based on State Machine Diagram: Available, OnHold, Booked, Sold, Blocked
/// Partition Key: EventId (seats are always queried by event)
/// </summary>
public class Seat : BaseEntity
{
    [JsonPropertyName("eventId")]
    [Required]
    public string EventId { get; set; } = string.Empty;

    [JsonPropertyName("manifestId")]
    [Required]
    public string ManifestId { get; set; } = string.Empty;

    [JsonPropertyName("seatNumber")]
    [Required]
    [MaxLength(50)]
    public string SeatNumber { get; set; } = string.Empty;

    [JsonPropertyName("row")]
    [MaxLength(50)]
    public string? Row { get; set; }

    [JsonPropertyName("section")]
    [MaxLength(50)]
    public string? Section { get; set; }

    [JsonPropertyName("status")]
    [Required]
    public SeatStatus Status { get; set; } = SeatStatus.Available;

    [JsonPropertyName("currentOfferId")]
    public string? CurrentOfferId { get; set; }

    // Embedded offer information (denormalized for performance)
    [JsonPropertyName("currentOffer")]
    public OfferInfo? CurrentOffer { get; set; }

    // For OnHold status - automatic release after timeout
    [JsonPropertyName("holdExpiresAt")]
    public DateTime? HoldExpiresAt { get; set; }

    [JsonPropertyName("heldByCustomerId")]
    public string? HeldByCustomerId { get; set; }

    // For Booked/Sold status - link to booking
    [JsonPropertyName("bookingId")]
    public string? BookingId { get; set; }

    [JsonIgnore]
    public override string PartitionKey => EventId;

    public Seat()
    {
        EntityType = nameof(Seat);
    }
}

public class OfferInfo
{
    [JsonPropertyName("offerId")]
    public string OfferId { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("price")]
    public decimal Price { get; set; }

    [JsonPropertyName("priceCategory")]
    public PriceCategory PriceCategory { get; set; }
}