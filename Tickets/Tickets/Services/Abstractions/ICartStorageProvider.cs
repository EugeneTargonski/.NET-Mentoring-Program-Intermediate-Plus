using Tickets.DTOs;

namespace Tickets.Services.Abstractions;

/// <summary>
/// Abstraction for cart storage mechanism (DIP compliance)
/// Allows switching between in-memory, Redis, database, etc.
/// </summary>
public interface ICartStorageProvider
{
    Task<List<CartItemDto>> GetCartItemsAsync(string cartId, CancellationToken cancellationToken = default);
    Task AddItemAsync(string cartId, CartItemDto item, CancellationToken cancellationToken = default);
    Task RemoveItemAsync(string cartId, string eventId, string seatId, CancellationToken cancellationToken = default);
    Task ClearCartAsync(string cartId, CancellationToken cancellationToken = default);
    Task<bool> CartExistsAsync(string cartId, CancellationToken cancellationToken = default);
}
