namespace Tickets.Services.Abstractions;

/// <summary>
/// Service interface for event caching operations
/// </summary>
public interface IEventCacheService
{
    void InvalidateCache();
}
