using Tickets.Data.Abstractions;
using Tickets.Domain.Entities;
using Tickets.Domain.Enums;
using Tickets.DTOs;
using Tickets.Services.Abstractions;

namespace Tickets.Services.Infrastructure;

/// <summary>
/// Service for creating bookings from cart
/// Follows SRP - only handles booking creation logic
/// </summary>
public class BookingService(
    IUnitOfWork unitOfWork,
    ISeatService seatService,
    IDateTimeProvider dateTimeProvider) : IBookingService
{
    public async Task<BookCartResponse> CreateBookingFromCartAsync(
        string cartId,
        List<CartItemDto> cartItems,
        string customerId,
        CancellationToken cancellationToken = default)
    {
        if (cartItems.Count == 0)
        {
            throw new InvalidOperationException("Cart is empty");
        }

        var totalAmount = cartItems.Sum(i => i.Amount);
        var bookings = new List<Booking>();
        var bookedSeats = new List<string>();

        // Validate all seats are available first
        foreach (var item in cartItems)
        {
            var isAvailable = await seatService.IsSeatAvailableAsync(
                item.SeatId,
                item.EventId,
                cancellationToken);

            if (!isAvailable)
            {
                throw new InvalidOperationException($"Seat {item.SeatId} is no longer available");
            }
        }

        // Create bookings and update seats
        foreach (var item in cartItems)
        {
            var booking = new Booking
            {
                Id = Guid.NewGuid().ToString(),
                SeatId = item.SeatId,
                EventId = item.EventId,
                CustomerId = customerId,
                OfferId = item.PriceId,
                Status = BookingStatus.Confirmed,
                Amount = item.Amount,
                ConfirmedAt = dateTimeProvider.UtcNow,
                CreatedAt = dateTimeProvider.UtcNow
            };

            await unitOfWork.Bookings.CreateAsync(booking, cancellationToken);
            bookings.Add(booking);

            // Update seat status
            await seatService.UpdateSeatStatusAsync(
                item.SeatId,
                item.EventId,
                SeatStatus.Booked,
                booking.Id,
                cancellationToken);

            var seat = await seatService.GetSeatAsync(item.SeatId, item.EventId, cancellationToken);
            if (seat != null)
            {
                bookedSeats.Add(seat.SeatNumber);
            }
        }

        // Create payment
        var payment = new Payment
        {
            Id = Guid.NewGuid().ToString(),
            BookingId = bookings.First().Id,
            CustomerId = customerId,
            Amount = totalAmount,
            Status = PaymentStatus.Pending,
            PaymentMethod = "CreditCard",
            CreatedAt = dateTimeProvider.UtcNow
        };

        await unitOfWork.Payments.CreateAsync(payment, cancellationToken);

        return new BookCartResponse(
            payment.Id,
            totalAmount,
            bookedSeats
        );
    }
}
