using Microsoft.AspNetCore.Mvc;
using Tickets.Services.Abstractions;

namespace Tickets.Controllers;

[ApiController]
[Route("api/venues")]
public class VenuesController(IVenueService venueService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetVenues(CancellationToken cancellationToken)
    {
        var venues = await venueService.GetAllVenuesAsync(cancellationToken);
        return Ok(venues);
    }

    [HttpGet("{venueId}/sections")]
    public async Task<IActionResult> GetVenueSections(
        string venueId, 
        CancellationToken cancellationToken)
    {
        var sections = await venueService.GetVenueSectionsAsync(venueId, cancellationToken);
        return Ok(sections);
    }
}
