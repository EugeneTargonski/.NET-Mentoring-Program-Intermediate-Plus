namespace Tickets.DTOs;

public record EventDto(
    string Id,
    string Name,
    string? Description,
    DateTime EventDate,
    DateTime? EventEndDate,
    string VenueId,
    string? Category,
    bool IsActive
);
