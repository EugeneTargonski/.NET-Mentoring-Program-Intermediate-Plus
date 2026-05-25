using Azure.Messaging.ServiceBus;
using System.Text.Json;
using Tickets.DTOs;
using Tickets.Services.Abstractions;

namespace Tickets.Services.Infrastructure;

/// <summary>
/// Service for sending notifications via Azure Service Bus
/// </summary>
public class NotificationService : INotificationService, IAsyncDisposable
{
    private readonly ServiceBusClient _client;
    private readonly ServiceBusSender _sender;
    private readonly string _queueName;

    public NotificationService(string connectionString, string queueName)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentException("Service Bus connection string cannot be null or empty", nameof(connectionString));
        }

        if (string.IsNullOrWhiteSpace(queueName))
        {
            throw new ArgumentException("Queue name cannot be null or empty", nameof(queueName));
        }

        _queueName = queueName;
        _client = new ServiceBusClient(connectionString);
        _sender = _client.CreateSender(queueName);
    }

    public async Task SendNotificationAsync(
        string operationName,
        string customerEmail,
        string customerName,
        decimal orderAmount,
        string orderSummary,
        string paymentId,
        string cartId,
        CancellationToken cancellationToken = default)
    {
        var notification = new NotificationMessage
        {
            OperationName = operationName,
            CustomerEmail = customerEmail,
            CustomerName = customerName,
            OrderAmount = orderAmount,
            OrderSummary = orderSummary,
            PaymentId = paymentId,
            CartId = cartId
        };

        var messageBody = JsonSerializer.Serialize(notification);
        var message = new ServiceBusMessage(messageBody)
        {
            ContentType = "application/json",
            MessageId = notification.NotificationId.ToString(),
            Subject = operationName
        };

        try
        {
            await _sender.SendMessageAsync(message, cancellationToken);
        }
        catch (Exception ex)
        {
            // Log error and rethrow - in production, consider dead-letter queue handling
            Console.WriteLine($"Error sending notification to Service Bus: {ex.Message}");
            throw;
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_sender != null)
        {
            await _sender.DisposeAsync();
        }

        if (_client != null)
        {
            await _client.DisposeAsync();
        }
    }
}
