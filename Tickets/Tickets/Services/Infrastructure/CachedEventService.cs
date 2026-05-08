using Microsoft.Extensions.Caching.Memory;
using Tickets.DTOs;
using Tickets.Services.Abstractions;

namespace Tickets.Services.Infrastructure;

/// <summary>
/// Decorator for IEventService that adds caching capabilities (OCP: Open for extension)
/// </summary>
public class CachedEventService : IEventService
{
    private const string EventsCacheKey = "all_events";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(30);

    private readonly IEventService _inner;
    private readonly IMemoryCache _cache;

    public CachedEventService(IEventService inner, IMemoryCache cache)
    {
        _inner = inner;
        _cache = cache;
    }

    public async Task<IEnumerable<EventDto>> GetAllEventsAsync(CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue(EventsCacheKey, out IEnumerable<EventDto>? cachedEvents) && cachedEvents != null)
        {
            return cachedEvents;
        }

        var events = await _inner.GetAllEventsAsync(cancellationToken);
        var eventList = events.ToList();

        _cache.Set(EventsCacheKey, eventList, CacheDuration);

        return eventList;
    }

    public Task<IEnumerable<EventSeatDto>> GetEventSeatsAsync(
        string eventId, 
        string sectionId, 
        CancellationToken cancellationToken = default)
    {
        return _inner.GetEventSeatsAsync(eventId, sectionId, cancellationToken);
    }
}
