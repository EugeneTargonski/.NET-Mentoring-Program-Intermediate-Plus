using Microsoft.Extensions.Logging;
using Tickets.Data.Abstractions;
using Tickets.Data.Repositories;
using Tickets.Domain.Entities;

namespace Tickets.Data.UnitOfWork;

/// <summary>
/// Cosmos DB implementation of IUnitOfWork
/// </summary>
public class CosmosUnitOfWork(CosmosDbContext context, ILoggerFactory loggerFactory) : IUnitOfWork
{
    private readonly CosmosDbContext _context = context ?? throw new ArgumentNullException(nameof(context));
    private readonly ILoggerFactory _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));

    // EventDb repositories
    private IRepository<Event>? _events;
    private IRepository<Venue>? _venues;
    private IRepository<Manifest>? _manifests;
    private IRepository<Offer>? _offers;

    // InventoryDb repositories
    private ISeatRepository? _seats;

    // TransactionDb repositories
    private IBookingRepository? _bookings;
    private IRepository<Payment>? _payments;

    // TicketDb repositories
    private IRepository<Ticket>? _tickets;

    #region EventDb Repositories

    public IRepository<Event> Events =>
        _events ??= new CosmosRepository<Event>(
            _context.GetEventDbContainer(CosmosDbContext.EventsContainerName),
            _loggerFactory.CreateLogger<CosmosRepository<Event>>());

    public IRepository<Venue> Venues =>
        _venues ??= new CosmosRepository<Venue>(
            _context.GetEventDbContainer(CosmosDbContext.VenuesContainerName),
            _loggerFactory.CreateLogger<CosmosRepository<Venue>>());

    public IRepository<Manifest> Manifests =>
        _manifests ??= new CosmosRepository<Manifest>(
            _context.GetEventDbContainer(CosmosDbContext.ManifestsContainerName),
            _loggerFactory.CreateLogger<CosmosRepository<Manifest>>());

    public IRepository<Offer> Offers =>
        _offers ??= new CosmosRepository<Offer>(
            _context.GetEventDbContainer(CosmosDbContext.OffersContainerName),
            _loggerFactory.CreateLogger<CosmosRepository<Offer>>());

    #endregion

    #region InventoryDb Repositories

    public ISeatRepository Seats =>
        _seats ??= new SeatRepository(
            _context.GetInventoryDbContainer(CosmosDbContext.SeatsContainerName),
            _loggerFactory.CreateLogger<CosmosRepository<Seat>>());

    #endregion

    #region TransactionDb Repositories

    public IBookingRepository Bookings =>
        _bookings ??= new BookingRepository(
            _context.GetTransactionDbContainer(CosmosDbContext.BookingsContainerName),
            _loggerFactory.CreateLogger<CosmosRepository<Booking>>());

    public IRepository<Payment> Payments =>
        _payments ??= new CosmosRepository<Payment>(
            _context.GetTransactionDbContainer(CosmosDbContext.PaymentsContainerName),
            _loggerFactory.CreateLogger<CosmosRepository<Payment>>());

    #endregion

    #region TicketDb Repositories

    public IRepository<Ticket> Tickets =>
        _tickets ??= new CosmosRepository<Ticket>(
            _context.GetTicketDbContainer(CosmosDbContext.TicketsContainerName),
            _loggerFactory.CreateLogger<CosmosRepository<Ticket>>());

    #endregion

    public void Dispose()
    {
        _context?.Dispose();
        GC.SuppressFinalize(this);
    }
}