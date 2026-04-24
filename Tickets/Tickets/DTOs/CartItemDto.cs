namespace Tickets.DTOs;

public record CartItemDto(
    string EventId,
    string SeatId,
    string PriceId,
    decimal Amount
);
