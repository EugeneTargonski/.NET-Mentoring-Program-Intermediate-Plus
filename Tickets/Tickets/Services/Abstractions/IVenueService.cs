using Tickets.DTOs;

namespace Tickets.Services.Abstractions;

/// <summary>
/// Service interface for venue-related operations
/// </summary>
public interface IVenueService
{
    Task<IEnumerable<VenueDto>> GetAllVenuesAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<VenueSectionDto>> GetVenueSectionsAsync(string venueId, CancellationToken cancellationToken = default);
}
