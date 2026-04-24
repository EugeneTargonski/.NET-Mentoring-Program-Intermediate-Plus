using Tickets.Data.Abstractions;
using Tickets.Domain.Entities;
using Tickets.Domain.Enums;
using Tickets.Services.Abstractions;

namespace Tickets.Services.Infrastructure;

/// <summary>
/// Service for managing seat status and availability
/// Follows SRP - only handles seat operations
/// </summary>
public class SeatService(IUnitOfWork unitOfWork) : ISeatService
{
    public async Task<Seat?> GetSeatAsync(
        string seatId,
        string eventId,
        CancellationToken cancellationToken = default)
    {
        return await unitOfWork.Seats.GetByIdAsync(seatId, eventId, cancellationToken);
    }

    public async Task UpdateSeatStatusAsync(
        string seatId,
        string eventId,
        SeatStatus status,
        string? bookingId = null,
        CancellationToken cancellationToken = default)
    {
        var seat = await unitOfWork.Seats.GetByIdAsync(seatId, eventId, cancellationToken) ?? throw new InvalidOperationException($"Seat {seatId} not found");
        seat.Status = status;
        seat.BookingId = status == SeatStatus.Available ? null : bookingId;

        await unitOfWork.Seats.UpdateAsync(seat, cancellationToken);
    }

    public async Task UpdateSeatsForBookingsAsync(
        IEnumerable<Booking> bookings,
        SeatStatus status,
        CancellationToken cancellationToken = default)
    {
        foreach (var booking in bookings)
        {
            await UpdateSeatStatusAsync(
                booking.SeatId,
                booking.EventId,
                status,
                status != SeatStatus.Available ? booking.Id : null,
                cancellationToken);
        }
    }

    public async Task<bool> IsSeatAvailableAsync(
        string seatId,
        string eventId,
        CancellationToken cancellationToken = default)
    {
        var seat = await GetSeatAsync(seatId, eventId, cancellationToken);
        return seat != null && seat.Status == SeatStatus.Available;
    }
}
