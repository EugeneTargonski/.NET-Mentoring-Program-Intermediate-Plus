namespace NotificationHandler.Models;

/// <summary>
/// Notification message received from Azure Service Bus
/// </summary>
public class NotificationMessage
{
    public Guid NotificationId { get; set; }
    public string OperationName { get; set; } = string.Empty;
    public DateTimeOffset Timestamp { get; set; }
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public decimal OrderAmount { get; set; }
    public string OrderSummary { get; set; } = string.Empty;
    public string PaymentId { get; set; } = string.Empty;
    public string CartId { get; set; } = string.Empty;
}
