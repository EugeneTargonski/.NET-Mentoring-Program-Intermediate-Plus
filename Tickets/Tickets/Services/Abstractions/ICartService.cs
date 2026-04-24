using Tickets.DTOs;

namespace Tickets.Services.Abstractions;

/// <summary>
/// Service interface for managing shopping cart operations
/// </summary>
public interface ICartService
{
    /// <summary>
    /// Retrieves a cart by its ID
    /// </summary>
    Task<CartDto> GetCartAsync(string cartId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds an item to the cart
    /// </summary>
    Task<CartDto> AddToCartAsync(
        string cartId,
        AddToCartRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes an item from the cart
    /// </summary>
    Task<CartDto> RemoveFromCartAsync(
        string cartId,
        string eventId,
        string seatId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Books all items in the cart and creates a booking with payment
    /// </summary>
    Task<BookCartResponse> BookCartAsync(
        string cartId,
        CancellationToken cancellationToken = default);
}
