using Tickets.Data.Abstractions;
using Tickets.Domain.Enums;
using Tickets.DTOs;
using Tickets.Services.Abstractions;

namespace Tickets.Services;

public class CartService(
    ICartStorageProvider storageProvider,
    IBookingService bookingService,
    IUnitOfWork unitOfWork) : ICartService
{
    public async Task<CartDto> GetCartAsync(string cartId, CancellationToken cancellationToken = default)
    {
        var items = await storageProvider.GetCartItemsAsync(cartId, cancellationToken);
        var totalAmount = items.Sum(i => i.Amount);

        return new CartDto(cartId, items, totalAmount);
    }

    public async Task<CartDto> AddToCartAsync(
        string cartId,
        AddToCartRequest request,
        CancellationToken cancellationToken = default)
    {
        // Validate offer exists
        var offer = await unitOfWork.Offers.GetByIdAsync(
            request.PriceId,
            request.PriceId,
            cancellationToken) ?? throw new InvalidOperationException($"Offer {request.PriceId} not found");

        // Validate seat exists and is available
        var seat = await unitOfWork.Seats.GetByIdAsync(
            request.SeatId,
            request.EventId,
            cancellationToken) ?? throw new InvalidOperationException($"Seat {request.SeatId} not found");

        if (seat.Status != SeatStatus.Available)
        {
            throw new InvalidOperationException($"Seat {request.SeatId} is not available");
        }

        // Add item to cart
        var cartItem = new CartItemDto(
            request.EventId,
            request.SeatId,
            request.PriceId,
            offer.Price
        );

        await storageProvider.AddItemAsync(cartId, cartItem, cancellationToken);

        // Return updated cart
        return await GetCartAsync(cartId, cancellationToken);
    }

    public async Task<CartDto> RemoveFromCartAsync(
        string cartId,
        string eventId,
        string seatId,
        CancellationToken cancellationToken = default)
    {
        await storageProvider.RemoveItemAsync(cartId, eventId, seatId, cancellationToken);
        return await GetCartAsync(cartId, cancellationToken);
    }

    public async Task<BookCartResponse> BookCartAsync(
        string cartId,
        CancellationToken cancellationToken = default)
    {
        var items = await storageProvider.GetCartItemsAsync(cartId, cancellationToken);

        // Generate customer ID (in real app, this would come from auth context)
        var customerId = $"customer-{Guid.NewGuid()}";

        // Delegate booking creation to BookingService
        var result = await bookingService.CreateBookingFromCartAsync(
            cartId,
            items,
            customerId,
            cancellationToken);

        // Clear cart after successful booking
        await storageProvider.ClearCartAsync(cartId, cancellationToken);

        return result;
    }
}
