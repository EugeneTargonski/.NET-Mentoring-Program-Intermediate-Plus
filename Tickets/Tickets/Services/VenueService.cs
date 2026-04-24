using Tickets.Data.Abstractions;
using Tickets.DTOs;
using Tickets.Services.Abstractions;

namespace Tickets.Services;

public class VenueService(IUnitOfWork unitOfWork) : IVenueService
{
    public async Task<IEnumerable<VenueDto>> GetAllVenuesAsync(CancellationToken cancellationToken = default)
    {
        var venues = await unitOfWork.Venues.GetAllAsync(cancellationToken);

        return venues.Select(v => new VenueDto(
            v.Id,
            v.Name,
            v.Address,
            v.City,
            v.Country,
            v.Capacity
        ));
    }

    public async Task<IEnumerable<VenueSectionDto>> GetVenueSectionsAsync(string venueId, CancellationToken cancellationToken = default)
    {
        var manifests = await unitOfWork.Manifests.QueryAsync(
            m => m.VenueId == venueId,
            venueId,
            cancellationToken);

        var manifest = manifests.FirstOrDefault();
        if (manifest == null)
        {
            return [];
        }

        var events = await unitOfWork.Events.QueryAsync(
            e => e.ManifestId == manifest.Id,
            cancellationToken: cancellationToken);

        var firstEvent = events.FirstOrDefault();
        if (firstEvent == null)
        {
            return [];
        }

        var seats = await unitOfWork.Seats.QueryAsync(
            s => s.EventId == firstEvent.Id,
            firstEvent.Id,
            cancellationToken);

        return seats
            .Where(s => !string.IsNullOrEmpty(s.Section))
            .GroupBy(s => s.Section!)
            .Select(g => new VenueSectionDto(g.Key, g.Count()))
            .OrderBy(s => s.Section);
    }
}
