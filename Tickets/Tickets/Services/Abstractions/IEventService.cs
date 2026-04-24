using Tickets.DTOs;

namespace Tickets.Services.Abstractions;

/// <summary>
/// Service interface for event-related operations
/// </summary>
public interface IEventService
{
    Task<IEnumerable<EventDto>> GetAllEventsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<EventSeatDto>> GetEventSeatsAsync(string eventId, string sectionId, CancellationToken cancellationToken = default);
}
