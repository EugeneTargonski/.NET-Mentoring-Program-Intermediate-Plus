namespace Tickets.DTOs;

public record VenueDto(
    string Id,
    string Name,
    string Address,
    string? City,
    string? Country,
    int Capacity
);
