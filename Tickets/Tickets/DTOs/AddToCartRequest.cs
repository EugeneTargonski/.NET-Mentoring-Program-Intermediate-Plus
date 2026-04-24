namespace Tickets.DTOs;

public record AddToCartRequest(
    string EventId,
    string SeatId,
    string PriceId
);
