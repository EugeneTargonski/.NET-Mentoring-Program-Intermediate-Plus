using System.Collections.Concurrent;
using Tickets.DTOs;
using Tickets.Services.Abstractions;

namespace Tickets.Services.Infrastructure;

/// <summary>
/// In-memory implementation of cart storage
/// Can be replaced with Redis, Cosmos DB, etc. without changing business logic
/// </summary>
public class InMemoryCartStorageProvider : ICartStorageProvider
{
    private static readonly ConcurrentDictionary<string, List<CartItemDto>> _carts = new();

    public Task<List<CartItemDto>> GetCartItemsAsync(string cartId, CancellationToken cancellationToken = default)
    {
        var items = _carts.GetOrAdd(cartId, _ => []);
        return Task.FromResult(items);
    }

    public Task AddItemAsync(string cartId, CartItemDto item, CancellationToken cancellationToken = default)
    {
        var items = _carts.GetOrAdd(cartId, _ => []);

        // Remove existing item for same seat (update scenario)
        var existingItem = items.FirstOrDefault(i =>
            i.EventId == item.EventId && i.SeatId == item.SeatId);

        if (existingItem != null)
        {
            items.Remove(existingItem);
        }

        items.Add(item);
        return Task.CompletedTask;
    }

    public Task RemoveItemAsync(
        string cartId,
        string eventId,
        string seatId,
        CancellationToken cancellationToken = default)
    {
        if (_carts.TryGetValue(cartId, out var items))
        {
            var item = items.FirstOrDefault(i =>
                i.EventId == eventId && i.SeatId == seatId);

            if (item != null)
            {
                items.Remove(item);
            }
        }

        return Task.CompletedTask;
    }

    public Task ClearCartAsync(string cartId, CancellationToken cancellationToken = default)
    {
        _carts.TryRemove(cartId, out _);
        return Task.CompletedTask;
    }

    public Task<bool> CartExistsAsync(string cartId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_carts.ContainsKey(cartId));
    }
}
