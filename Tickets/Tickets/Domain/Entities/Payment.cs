using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Tickets.Domain.Enums;

namespace Tickets.Domain.Entities;

/// <summary>
/// Represents payment transaction
/// Stored in TransactionDb as separate document (not embedded)
/// Partition Key: BookingId (for payment queries by booking)
/// </summary>
public class Payment : BaseEntity
{
    [JsonPropertyName("bookingId")]
    [Required]
    public string BookingId { get; set; } = string.Empty;

    [JsonPropertyName("customerId")]
    [Required]
    public string CustomerId { get; set; } = string.Empty;

    [JsonPropertyName("amount")]
    [Required]
    [Range(0.01, 1000000)]
    public decimal Amount { get; set; }

    [JsonPropertyName("status")]
    [Required]
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

    [JsonPropertyName("paymentMethod")]
    [Required]
    [MaxLength(50)]
    public string PaymentMethod { get; set; } = string.Empty;

    [JsonPropertyName("transactionId")]
    [MaxLength(200)]
    public string? TransactionId { get; set; }

    [JsonPropertyName("paymentGatewayReference")]
    [MaxLength(200)]
    public string? PaymentGatewayReference { get; set; }

    [JsonPropertyName("processedAt")]
    public DateTime? ProcessedAt { get; set; }

    [JsonPropertyName("confirmedAt")]
    public DateTime? ConfirmedAt { get; set; }

    [JsonPropertyName("errorMessage")]
    [MaxLength(2000)]
    public string? ErrorMessage { get; set; }

    [JsonIgnore]
    public override string PartitionKey => BookingId;

    public Payment()
    {
        EntityType = nameof(Payment);
    }
}