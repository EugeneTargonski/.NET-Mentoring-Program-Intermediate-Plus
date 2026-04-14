using Tickets.Domain.Entities;

namespace Tickets.Data.Abstractions;

/// <summary>
/// Database-agnostic Unit of Work pattern
/// Abstracts away the underlying data store (Cosmos DB, SQL Server, etc.)
/// </summary>
public interface IUnitOfWork : IDisposable
{
    // EventDb repositories
    IRepository<Event> Events { get; }
    IRepository<Venue> Venues { get; }
    IRepository<Manifest> Manifests { get; }
    IRepository<Offer> Offers { get; }

    // InventoryDb repositories
    ISeatRepository Seats { get; }

    // TransactionDb repositories
    IBookingRepository Bookings { get; }
    IRepository<Payment> Payments { get; }

    // TicketDb repositories
    IRepository<Ticket> Tickets { get; }
}