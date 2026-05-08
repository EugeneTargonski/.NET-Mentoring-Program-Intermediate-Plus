using Microsoft.Extensions.Caching.Memory;
using Tickets.Data.Abstractions;
using Tickets.DTOs;
using Tickets.Services.Abstractions;

namespace Tickets.Services;

public class EventService(IUnitOfWork unitOfWork, IMemoryCache cache) : IEventService, IEventCacheService
{
    private const string EventsCacheKey = "all_events";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(30);

    public async Task<IEnumerable<EventDto>> GetAllEventsAsync(CancellationToken cancellationToken = default)
    {
        if (cache.TryGetValue(EventsCacheKey, out IEnumerable<EventDto>? cachedEvents) && cachedEvents != null)
        {
            return cachedEvents;
        }

        var events = await unitOfWork.Events.GetAllAsync(cancellationToken);

        var eventDtos = events.Select(e => new EventDto(
            e.Id,
            e.Name,
            e.Description,
            e.EventDate,
            e.EventEndDate,
            e.VenueId,
            e.Category,
            e.IsActive
        )).ToList();

        cache.Set(EventsCacheKey, eventDtos, CacheDuration);

        return eventDtos;
    }

    public async Task<IEnumerable<EventSeatDto>> GetEventSeatsAsync(
        string eventId, 
        string sectionId, 
        CancellationToken cancellationToken = default)
    {
        var seats = await unitOfWork.Seats.QueryAsync(
            s => s.EventId == eventId && s.Section == sectionId,
            eventId,
            cancellationToken);

        var result = new List<EventSeatDto>();

        foreach (var seat in seats)
        {
            PriceOptionDto? priceOption = null;

            if (!string.IsNullOrEmpty(seat.CurrentOfferId))
            {
                        var offer = await unitOfWork.Offers.GetByIdAsync(
                    seat.CurrentOfferId, 
                    seat.CurrentOfferId, 
                    cancellationToken);

                if (offer != null)
                {
                    priceOption = new PriceOptionDto(
                        offer.Id,
                        offer.Name,
                        offer.Price
                    );
                }
            }

            result.Add(new EventSeatDto(
                seat.Id,
                seat.Section ?? string.Empty,
                seat.Row,
                seat.SeatNumber,
                seat.Status.ToString(),
                priceOption
            ));
        }

        return result;
    }

    public void InvalidateCache()
    {
        cache.Remove(EventsCacheKey);
    }
}
