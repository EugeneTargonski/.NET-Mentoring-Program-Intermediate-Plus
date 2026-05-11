using Microsoft.AspNetCore.Mvc;
using Tickets.Infrastructure;
using Tickets.Services.Abstractions;

namespace Tickets.Controllers;

[ApiController]
[Route("api/events")]
public class EventsController : ControllerBase
{
    private readonly IEventService _eventService;

    public EventsController(IEventService eventService)
    {
        _eventService = eventService;
    }

    /// <summary>
    /// Get all events with HTTP caching support
    /// Client can cache response for 5 minutes and use ETag for validation
    /// </summary>
    [HttpGet]
    [HttpCache(durationSeconds: 300)] // 5 minutes client-side cache
    public async Task<IActionResult> GetEvents(CancellationToken cancellationToken)
    {
        var events = await _eventService.GetAllEventsAsync(cancellationToken);
        return Ok(events);
    }

    /// <summary>
    /// Get event seats with HTTP caching support
    /// Cache varies by eventId and sectionId in the URL
    /// </summary>
    [HttpGet("{eventId}/sections/{sectionId}/seats")]
    [HttpCache(durationSeconds: 180, varyByQueryKeys: true)] // 3 minutes client-side cache
    public async Task<IActionResult> GetEventSeats(
        string eventId,
        string sectionId,
        CancellationToken cancellationToken)
    {
        var seats = await _eventService.GetEventSeatsAsync(eventId, sectionId, cancellationToken);
        return Ok(seats);
    }
}
