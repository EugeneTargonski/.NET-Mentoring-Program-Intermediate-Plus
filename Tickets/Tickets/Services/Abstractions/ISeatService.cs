using Tickets.Domain.Entities;
using Tickets.Domain.Enums;

namespace Tickets.Services.Abstractions;

/// <summary>
/// Abstraction for seat management (SRP compliance)
/// Centralizes seat status updates
/// </summary>
public interface ISeatService
{
    Task<Seat?> GetSeatAsync(string seatId, string eventId, CancellationToken cancellationToken = default);

    Task UpdateSeatStatusAsync(
        string seatId,
        string eventId,
        SeatStatus status,
        string? bookingId = null,
        CancellationToken cancellationToken = default);

    Task UpdateSeatsForBookingsAsync(
        IEnumerable<Booking> bookings,
        SeatStatus status,
        CancellationToken cancellationToken = default);

    Task<bool> IsSeatAvailableAsync(
        string seatId,
        string eventId,
        CancellationToken cancellationToken = default);
}
