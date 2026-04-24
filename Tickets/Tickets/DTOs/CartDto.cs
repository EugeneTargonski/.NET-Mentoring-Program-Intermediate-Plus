namespace Tickets.DTOs;

public record CartDto(
    string CartId,
    List<CartItemDto> Items,
    decimal TotalAmount
);
