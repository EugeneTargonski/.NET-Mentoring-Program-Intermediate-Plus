using Tickets.Domain.Entities;

namespace Tickets.Data.Abstractions;

/// <summary>
/// Database-agnostic booking repository interface
/// </summary>
public interface IBookingRepository : IRepository<Booking>
{
    Task<IEnumerable<Booking>> GetBookingsByCustomerIdAsync(string customerId, CancellationToken cancellationToken = default);
}