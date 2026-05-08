using Microsoft.Extensions.Caching.Memory;
using Tickets.Services.Abstractions;

namespace Tickets.Services.Infrastructure;

/// <summary>
/// Service for managing event cache (SRP: Single responsibility - caching only)
/// </summary>
public class EventCacheService : IEventCacheService
{
    private const string EventsCacheKey = "all_events";
    private readonly IMemoryCache _cache;

    public EventCacheService(IMemoryCache cache)
    {
        _cache = cache;
    }

    public void InvalidateCache()
    {
        _cache.Remove(EventsCacheKey);
    }
}
