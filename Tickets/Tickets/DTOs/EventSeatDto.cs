namespace Tickets.DTOs;

public record EventSeatDto(
    string SeatId,
    string SectionId,
    string? Row,
    string SeatNumber,
    string Status,
    PriceOptionDto? PriceOption
);
