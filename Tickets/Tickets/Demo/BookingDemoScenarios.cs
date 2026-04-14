using Microsoft.Extensions.Logging;
using Tickets.Data.Abstractions;
using Tickets.Domain.Entities;
using Tickets.Domain.Enums;

namespace Tickets.Demo;

/// <summary>
/// Responsibility: Demonstrate Booking Service operations (TransactionDb)
/// </summary>
public class BookingDemoScenarios(IUnitOfWork unitOfWork, ILogger<BookingDemoScenarios> logger)
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    private readonly ILogger<BookingDemoScenarios> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task RunAllAsync()
    {
        await CreateBookingAsync();
        await GetCustomerBookingsAsync();
    }

    private async Task CreateBookingAsync()
    {
        _logger.LogInformation("--- Demo: Create Booking (TransactionDb) ---");

        var events = await _unitOfWork.Events.GetAllAsync();
        var firstEvent = events.FirstOrDefault();
        
        if (firstEvent == null)
        {
            _logger.LogWarning("No events found");
            return;
        }

        var heldSeats = await _unitOfWork.Seats.QueryAsync(
            s => s.Status == SeatStatus.OnHold, 
            partitionKey: firstEvent.Id);
        var heldSeat = heldSeats.FirstOrDefault();
        
        if (heldSeat == null)
        {
            _logger.LogWarning("No held seats found");
            return;
        }

        var booking = new Booking
        {
            SeatId = heldSeat.Id,
            EventId = firstEvent.Id,
            CustomerId = "customer123",
            OfferId = heldSeat.CurrentOfferId ?? "",
            Amount = heldSeat.CurrentOffer?.Price ?? 0,
            Status = BookingStatus.Pending,
            SeatInfo = new SeatInfo { SeatId = heldSeat.Id, SeatNumber = heldSeat.SeatNumber },
            EventInfo = new EventInfo { EventId = firstEvent.Id, Name = firstEvent.Name, EventDate = firstEvent.EventDate }
        };

        await _unitOfWork.Bookings.CreateAsync(booking);
        
        // Reserve seat (transition from OnHold to Booked)
        await _unitOfWork.Seats.ReserveSeatAsync(heldSeat.Id, firstEvent.Id, booking.Id);

        _logger.LogInformation("Created booking in TransactionDb, updated seat in InventoryDb");
    }

    private async Task GetCustomerBookingsAsync()
    {
        _logger.LogInformation("--- Demo: Get Customer Bookings (TransactionDb) ---");

        var bookings = await _unitOfWork.Bookings.GetBookingsByCustomerIdAsync("customer123");
        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("Found {Count} bookings in TransactionDb", bookings.Count());
        }
    }
}