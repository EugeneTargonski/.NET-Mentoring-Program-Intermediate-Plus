namespace Tickets.DTOs;

/// <summary>
/// Message sent to Azure Service Bus for email notifications
/// </summary>
public record NotificationMessage
{
    /// <summary>
    /// Unique tracking identifier for the notification
    /// </summary>
    public Guid NotificationId { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Operation that triggered the notification
    /// </summary>
    public required string OperationName { get; init; }

    /// <summary>
    /// Timestamp when the notification was created
    /// </summary>
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Customer email address
    /// </summary>
    public required string CustomerEmail { get; init; }

    /// <summary>
    /// Customer name
    /// </summary>
    public required string CustomerName { get; init; }

    /// <summary>
    /// Order amount
    /// </summary>
    public decimal OrderAmount { get; init; }

    /// <summary>
    /// Order summary (list of booked seats)
    /// </summary>
    public required string OrderSummary { get; init; }

    /// <summary>
    /// Payment ID associated with the order
    /// </summary>
    public required string PaymentId { get; init; }

    /// <summary>
    /// Cart ID that was processed
    /// </summary>
    public required string CartId { get; init; }
}
