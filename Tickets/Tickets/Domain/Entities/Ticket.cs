using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Tickets.Domain.Entities;

/// <summary>
/// Represents an issued ticket
/// Stored in TicketDb as separate document
/// Partition Key: BookingId (for ticket queries by booking)
/// </summary>
public class Ticket : BaseEntity
{
    [JsonPropertyName("bookingId")]
    [Required]
    public string BookingId { get; set; } = string.Empty;

    [JsonPropertyName("customerId")]
    [Required]
    public string CustomerId { get; set; } = string.Empty;

    [JsonPropertyName("eventId")]
    [Required]
    public string EventId { get; set; } = string.Empty;

    [JsonPropertyName("ticketNumber")]
    [Required]
    [MaxLength(100)]
    public string TicketNumber { get; set; } = string.Empty;

    [JsonPropertyName("qrCode")]
    [Required]
    [MaxLength(500)]
    public string QrCode { get; set; } = string.Empty;

    [JsonPropertyName("isUsed")]
    public bool IsUsed { get; set; } = false;

    [JsonPropertyName("usedAt")]
    public DateTime? UsedAt { get; set; }

    [JsonPropertyName("sentAt")]
    public DateTime? SentAt { get; set; }

    // Embedded event info for display
    [JsonPropertyName("eventInfo")]
    public EventInfo? EventInfo { get; set; }

    // Embedded seat info for display
    [JsonPropertyName("seatInfo")]
    public SeatInfo? SeatInfo { get; set; }

    [JsonIgnore]
    public override string PartitionKey => BookingId;

    public Ticket()
    {
        EntityType = nameof(Ticket);
    }
}