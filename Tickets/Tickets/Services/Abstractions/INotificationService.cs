namespace Tickets.Services.Abstractions;

/// <summary>
/// Service for sending email notifications via message queue
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Sends a notification message to the queue
    /// </summary>
    /// <param name="operationName">Operation that triggered the notification</param>
    /// <param name="customerEmail">Customer email address</param>
    /// <param name="customerName">Customer name</param>
    /// <param name="orderAmount">Total order amount</param>
    /// <param name="orderSummary">Summary of the order</param>
    /// <param name="paymentId">Payment ID</param>
    /// <param name="cartId">Cart ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task SendNotificationAsync(
        string operationName,
        string customerEmail,
        string customerName,
        decimal orderAmount,
        string orderSummary,
        string paymentId,
        string cartId,
        CancellationToken cancellationToken = default);
}
