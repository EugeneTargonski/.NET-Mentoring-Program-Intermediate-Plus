namespace Tickets.DTOs;

public record PaymentStatusResponse(
    string PaymentId,
    string Status,
    decimal Amount,
    DateTime? ProcessedAt,
    string? ErrorMessage
);
