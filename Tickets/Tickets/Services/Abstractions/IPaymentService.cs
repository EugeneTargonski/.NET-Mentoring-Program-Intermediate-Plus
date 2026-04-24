using Tickets.DTOs;

namespace Tickets.Services.Abstractions;

/// <summary>
/// Service interface for managing payment operations
/// </summary>
public interface IPaymentService
{
    /// <summary>
    /// Retrieves the current status of a payment
    /// </summary>
    Task<PaymentStatusResponse> GetPaymentStatusAsync(
        string paymentId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks a payment as completed and updates related seat statuses to Sold
    /// </summary>
    Task<PaymentStatusResponse> CompletePaymentAsync(
        string paymentId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks a payment as failed and releases related seats back to Available
    /// </summary>
    Task<PaymentStatusResponse> FailPaymentAsync(
        string paymentId,
        CancellationToken cancellationToken = default);
}
