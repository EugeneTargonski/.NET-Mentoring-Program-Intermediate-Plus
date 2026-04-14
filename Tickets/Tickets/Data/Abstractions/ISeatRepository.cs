using Tickets.Domain.Entities;
using Tickets.Domain.Enums;

namespace Tickets.Data.Abstractions;

/// <summary>
/// Database-agnostic seat repository interface
/// </summary>
public interface ISeatRepository : IRepository<Seat>
{
    Task<IEnumerable<Seat>> GetAvailableSeatsByEventIdAsync(string eventId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Seat>> GetAvailableSeatsWithOffersByEventIdAsync(string eventId, CancellationToken cancellationToken = default);
    Task<bool> HoldSeatAsync(string seatId, string eventId, string customerId, DateTime holdExpiresAt, CancellationToken cancellationToken = default);
    Task<int> ReleaseExpiredHoldsAsync(string eventId, CancellationToken cancellationToken = default);
    Task<bool> ReserveSeatAsync(string seatId, string eventId, string bookingId, CancellationToken cancellationToken = default);
}