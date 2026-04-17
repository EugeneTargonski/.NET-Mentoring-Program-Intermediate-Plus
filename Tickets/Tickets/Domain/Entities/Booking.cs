using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Tickets.Domain.Enums;

namespace Tickets.Domain.Entities;

/// <summary>
/// Represents a customer's seat reservation
/// Partition Key: CustomerId (bookings are queried by customer)
/// </summary>
public class Booking : BaseEntity
{
    [JsonPropertyName("seatId")]
    [Required]
    public string SeatId { get; set; } = string.Empty;

    [JsonPropertyName("eventId")]
    [Required]
    public string EventId { get; set; } = string.Empty;

    [JsonPropertyName("customerId")]
    [Required]
    [MaxLength(450)]
    public string CustomerId { get; set; } = string.Empty;

    [JsonPropertyName("offerId")]
    [Required]
    public string OfferId { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    [Required]
    public BookingStatus Status { get; set; } = BookingStatus.Pending;

    [JsonPropertyName("amount")]
    [Required]
    [Range(0.01, 1000000)]
    public decimal Amount { get; set; }

    [JsonPropertyName("confirmedAt")]
    public DateTime? ConfirmedAt { get; set; }

    [JsonPropertyName("cancelledAt")]
    public DateTime? CancelledAt { get; set; }

    [JsonPropertyName("cancellationReason")]
    [MaxLength(2000)]
    public string? CancellationReason { get; set; }

    // Embedded seat information (denormalized)
    [JsonPropertyName("seatInfo")]
    public SeatInfo? SeatInfo { get; set; }

    // Embedded event information (denormalized)
    [JsonPropertyName("eventInfo")]
    public EventInfo? EventInfo { get; set; }

    // Embedded payment information
    [JsonPropertyName("payment")]
    public Payment? Payment { get; set; }

    // Embedded ticket information
    [JsonPropertyName("ticket")]
    public Ticket? Ticket { get; set; }

    [JsonIgnore]
    public override string PartitionKey => CustomerId;

    public Booking()
    {
        EntityType = nameof(Booking);
    }
}