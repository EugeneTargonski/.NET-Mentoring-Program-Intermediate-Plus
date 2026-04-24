using Microsoft.AspNetCore.Mvc;
using Tickets.Services.Abstractions;

namespace Tickets.Controllers;

[ApiController]
[Route("api/venues")]
public class VenuesController : ControllerBase
{
    private readonly IVenueService _venueService;

    public VenuesController(IVenueService venueService)
    {
        _venueService = venueService;
    }

    [HttpGet]
    public async Task<IActionResult> GetVenues(CancellationToken cancellationToken)
    {
        var venues = await _venueService.GetAllVenuesAsync(cancellationToken);
        return Ok(venues);
    }

    [HttpGet("{venueId}/sections")]
    public async Task<IActionResult> GetVenueSections(
        string venueId, 
        CancellationToken cancellationToken)
    {
        var sections = await _venueService.GetVenueSectionsAsync(venueId, cancellationToken);
        return Ok(sections);
    }
}
