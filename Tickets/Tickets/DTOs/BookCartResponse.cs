namespace Tickets.DTOs;

public record BookCartResponse(
    string PaymentId,
    decimal TotalAmount,
    List<string> BookedSeats
);
