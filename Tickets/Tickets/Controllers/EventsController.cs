using Microsoft.AspNetCore.Mvc;
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

    [HttpGet]
    public async Task<IActionResult> GetEvents(CancellationToken cancellationToken)
    {
        var events = await _eventService.GetAllEventsAsync(cancellationToken);
        return Ok(events);
    }

    [HttpGet("{eventId}/sections/{sectionId}/seats")]
    public async Task<IActionResult> GetEventSeats(
        string eventId,
        string sectionId,
        CancellationToken cancellationToken)
    {
        var seats = await _eventService.GetEventSeatsAsync(eventId, sectionId, cancellationToken);
        return Ok(seats);
    }
}
