using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Hosting;
using NotificationHandler.Models;
using System.Text.Json;

namespace NotificationHandler.Services;

/// <summary>
/// Background service that processes notification messages from Azure Service Bus
/// Uses console logging for status tracking (no database)
/// </summary>
public class NotificationProcessorService : BackgroundService
{
    private readonly ServiceBusProcessor _processor;
    private readonly IEmailProvider _emailProvider;

    public NotificationProcessorService(
        string serviceBusConnectionString,
        string queueName,
        IEmailProvider emailProvider)
    {
        var client = new ServiceBusClient(serviceBusConnectionString);
        _processor = client.CreateProcessor(queueName, new ServiceBusProcessorOptions
        {
            AutoCompleteMessages = false,
            MaxConcurrentCalls = 1
        });

        _emailProvider = emailProvider ?? throw new ArgumentNullException(nameof(emailProvider));

        _processor.ProcessMessageAsync += ProcessMessageAsync;
        _processor.ProcessErrorAsync += ProcessErrorAsync;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Console.WriteLine("Notification Handler started. Waiting for messages...");
        await _processor.StartProcessingAsync(stoppingToken);

        // Keep the service running until cancellation is requested
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task ProcessMessageAsync(ProcessMessageEventArgs args)
    {
        var messageBody = args.Message.Body.ToString();
        Console.WriteLine($"\n[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] Received message: {args.Message.MessageId}");

        try
        {
            // Deserialize the notification message
            var notification = JsonSerializer.Deserialize<NotificationMessage>(messageBody);
            if (notification == null)
            {
                throw new InvalidOperationException("Failed to deserialize notification message");
            }

            Console.WriteLine($"📧 Processing notification {notification.NotificationId} for {notification.CustomerEmail}");
            Console.WriteLine($"   Operation: {notification.OperationName}");
            Console.WriteLine($"   Status: InProgress");

            // Prepare email content
            var subject = notification.OperationName;
            var htmlContent = BuildEmailContent(notification);

            // Send email via email provider
            var success = await _emailProvider.SendEmailAsync(
                notification.CustomerEmail,
                notification.CustomerName,
                subject,
                htmlContent,
                args.CancellationToken);

            if (success)
            {
                // Complete the message (remove from queue)
                await args.CompleteMessageAsync(args.Message);
                Console.WriteLine($"✓ Notification {notification.NotificationId} processed successfully");
                Console.WriteLine($"   Status: Sent");
            }
            else
            {
                // Abandon the message (will be retried)
                await args.AbandonMessageAsync(args.Message);
                Console.WriteLine($"✗ Notification {notification.NotificationId} failed to send");
                Console.WriteLine($"   Status: Failed (will retry)");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Error processing message: {ex.Message}");
            Console.WriteLine($"   Status: Failed (exception)");

            // Abandon the message (will be retried)
            await args.AbandonMessageAsync(args.Message);
        }
    }

    private string BuildEmailContent(NotificationMessage notification)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #4CAF50; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f9f9f9; }}
        .footer {{ padding: 20px; text-align: center; font-size: 12px; color: #666; }}
        .amount {{ font-size: 24px; font-weight: bold; color: #4CAF50; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Order Confirmation</h1>
        </div>
        <div class='content'>
            <p>Dear {notification.CustomerName},</p>
            <p>Thank you for your order! Your tickets have been successfully booked.</p>

            <h3>Order Details:</h3>
            <p><strong>Order Summary:</strong> {notification.OrderSummary}</p>
            <p><strong>Total Amount:</strong> <span class='amount'>${notification.OrderAmount:F2}</span></p>
            <p><strong>Payment ID:</strong> {notification.PaymentId}</p>
            <p><strong>Order Date:</strong> {notification.Timestamp:yyyy-MM-dd HH:mm:ss} UTC</p>

            <p>Your tickets will be available in your account shortly.</p>
        </div>
        <div class='footer'>
            <p>This is an automated message. Please do not reply to this email.</p>
            <p>&copy; 2024 Ticketing System. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
    }

    private Task ProcessErrorAsync(ProcessErrorEventArgs args)
    {
        Console.WriteLine($"Error processing message: {args.Exception.Message}");
        return Task.CompletedTask;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("Notification Handler stopping...");
        await _processor.StopProcessingAsync(cancellationToken);
        await _processor.DisposeAsync();
        await base.StopAsync(cancellationToken);
    }
}
