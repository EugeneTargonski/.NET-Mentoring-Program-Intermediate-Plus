using Tickets.Data.Abstractions;
using Tickets.Domain.Enums;
using Tickets.DTOs;
using Tickets.Services.Abstractions;

namespace Tickets.Services;

public class PaymentService(
    IUnitOfWork unitOfWork,
    ISeatService seatService,
    IDateTimeProvider dateTimeProvider) : IPaymentService
{
    public async Task<PaymentStatusResponse> GetPaymentStatusAsync(
        string paymentId,
        CancellationToken cancellationToken = default)
    {
        var payments = await unitOfWork.Payments.QueryAsync(
            p => p.Id == paymentId,
            cancellationToken: cancellationToken);

        var payment = payments.FirstOrDefault() ?? throw new InvalidOperationException($"Payment {paymentId} not found");
        
        return new PaymentStatusResponse(
            payment.Id,
            payment.Status.ToString(),
            payment.Amount,
            payment.ProcessedAt,
            payment.ErrorMessage
        );
    }

    public async Task<PaymentStatusResponse> CompletePaymentAsync(
        string paymentId,
        CancellationToken cancellationToken = default)
    {
        var payment = await GetPaymentEntityAsync(paymentId, cancellationToken);

        // Update payment status
        payment.Status = PaymentStatus.Confirmed;
        payment.ProcessedAt = dateTimeProvider.UtcNow;
        payment.ConfirmedAt = dateTimeProvider.UtcNow;
        await unitOfWork.Payments.UpdateAsync(payment, cancellationToken);

        // Get related bookings
        var bookings = await GetBookingsForPaymentAsync(payment, cancellationToken);

        // Delegate seat status update to SeatService (SRP compliance)
        await seatService.UpdateSeatsForBookingsAsync(bookings, SeatStatus.Sold, cancellationToken);

        return new PaymentStatusResponse(
            payment.Id,
            payment.Status.ToString(),
            payment.Amount,
            payment.ProcessedAt,
            payment.ErrorMessage
        );
    }

    public async Task<PaymentStatusResponse> FailPaymentAsync(
        string paymentId,
        CancellationToken cancellationToken = default)
    {
        var payment = await GetPaymentEntityAsync(paymentId, cancellationToken);

        // Update payment status
        payment.Status = PaymentStatus.Failed;
        payment.ProcessedAt = dateTimeProvider.UtcNow;
        payment.ErrorMessage = "Payment failed";
        await unitOfWork.Payments.UpdateAsync(payment, cancellationToken);

        // Get related bookings
        var bookings = await GetBookingsForPaymentAsync(payment, cancellationToken);

        // Delegate seat status update to SeatService (SRP compliance)
        await seatService.UpdateSeatsForBookingsAsync(bookings, SeatStatus.Available, cancellationToken);

        return new PaymentStatusResponse(
            payment.Id,
            payment.Status.ToString(),
            payment.Amount,
            payment.ProcessedAt,
            payment.ErrorMessage
        );
    }

    // Private helper methods to reduce duplication (DRY principle)
    private async Task<Domain.Entities.Payment> GetPaymentEntityAsync(
        string paymentId,
        CancellationToken cancellationToken)
    {
        var payments = await unitOfWork.Payments.QueryAsync(
            p => p.Id == paymentId,
            cancellationToken: cancellationToken);

        var payment = payments.FirstOrDefault() ?? throw new InvalidOperationException($"Payment {paymentId} not found");

        return payment;
    }

    private async Task<List<Domain.Entities.Booking>> GetBookingsForPaymentAsync(
        Domain.Entities.Payment payment,
        CancellationToken cancellationToken)
    {
        var bookings = await unitOfWork.Bookings.QueryAsync(
            b => b.Id == payment.BookingId,
            payment.CustomerId,
            cancellationToken);

        return bookings.ToList();
    }
}
