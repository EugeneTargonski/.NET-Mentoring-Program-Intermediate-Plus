using Tickets.Domain.Entities;

namespace Tickets.Data.Abstractions;

/// <summary>
/// Database-agnostic booking repository interface
/// </summary>
public interface IBookingRepository : IRepository<Booking>
{
    Task<IEnumerable<Booking>> GetBookingsByCustomerIdAsync(string customerId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Booking>> GetBookingsByEventIdAsync(string eventId, CancellationToken cancellationToken = default);
    Task<bool> ConfirmBookingAsync(string bookingId, string customerId, CancellationToken cancellationToken = default);
    Task<bool> CancelBookingAsync(string bookingId, string customerId, string reason, CancellationToken cancellationToken = default);
}