namespace Tickets.DTOs;

public record PaymentDto(
    string Id,
    string BookingId,
    decimal Amount,
    string Status,
    DateTime CreatedAt,
    DateTime? ProcessedAt
);
