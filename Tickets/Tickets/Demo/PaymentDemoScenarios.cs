using Microsoft.Extensions.Logging;
using Tickets.Data.Abstractions;
using Tickets.Domain.Entities;
using Tickets.Domain.Enums;

namespace Tickets.Demo;

/// <summary>
/// Responsibility: Demonstrate Payment Service operations (TransactionDb)
/// </summary>
public class PaymentDemoScenarios(IUnitOfWork unitOfWork, ILogger<PaymentDemoScenarios> logger)
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    private readonly ILogger<PaymentDemoScenarios> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task RunAllAsync()
    {
        await ProcessPaymentAsync();
    }

    private async Task ProcessPaymentAsync()
    {
        _logger.LogInformation("--- Demo: Process Payment (TransactionDb + InventoryDb) ---");

        var bookings = await _unitOfWork.Bookings.GetBookingsByCustomerIdAsync("customer123");
        var pendingBooking = bookings.FirstOrDefault(b => b.Status == BookingStatus.Pending);
        
        if (pendingBooking == null)
        {
            _logger.LogWarning("No pending bookings found");
            return;
        }

        var payment = new Payment
        {
            BookingId = pendingBooking.Id,
            CustomerId = pendingBooking.CustomerId,
            Amount = pendingBooking.Amount,
            Status = PaymentStatus.Confirmed,
            PaymentMethod = "CreditCard",
            TransactionId = Guid.NewGuid().ToString(),
            ConfirmedAt = DateTime.UtcNow
        };

        await _unitOfWork.Payments.CreateAsync(payment);

        pendingBooking.Status = BookingStatus.Paid;
        await _unitOfWork.Bookings.UpdateAsync(pendingBooking);

        // Mark seat as sold
        var seat = await _unitOfWork.Seats.GetByIdAsync(pendingBooking.SeatId, pendingBooking.EventId);
        if (seat != null)
        {
            seat.Status = SeatStatus.Sold;
            await _unitOfWork.Seats.UpdateAsync(seat);
        }

        _logger.LogInformation("Payment saved in TransactionDb, seat marked sold in InventoryDb");
    }
}