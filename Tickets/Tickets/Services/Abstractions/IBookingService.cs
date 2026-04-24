using Tickets.DTOs;

namespace Tickets.Services.Abstractions;

/// <summary>
/// Abstraction for booking creation from cart (SRP compliance)
/// Separates cart management from booking logic
/// </summary>
public interface IBookingService
{
    Task<BookCartResponse> CreateBookingFromCartAsync(
        string cartId,
        List<CartItemDto> cartItems,
        string customerId,
        CancellationToken cancellationToken = default);
}
