using Microsoft.AspNetCore.Mvc;
using Tickets.DTOs;
using Tickets.Services.Abstractions;

namespace Tickets.Controllers;

[ApiController]
[Route("api/orders")]
public class OrdersController(ICartService cartService) : ControllerBase
{
    [HttpGet("carts/{cartId}")]
    public async Task<IActionResult> GetCart(
        string cartId, 
        CancellationToken cancellationToken)
    {
        var cart = await cartService.GetCartAsync(cartId, cancellationToken);
        return Ok(cart);
    }

    [HttpPost("carts/{cartId}/items")]
    public async Task<IActionResult> AddToCart(
        string cartId,
        [FromBody] AddToCartRequest request,
        CancellationToken cancellationToken)
    {
        var cart = await cartService.AddToCartAsync(cartId, request, cancellationToken);
        return CreatedAtAction(nameof(GetCart), new { cartId }, cart);
    }

    [HttpDelete("carts/{cartId}/events/{eventId}/seats/{seatId}")]
    public async Task<IActionResult> RemoveFromCart(
        string cartId,
        string eventId,
        string seatId,
        CancellationToken cancellationToken)
    {
        await cartService.RemoveFromCartAsync(cartId, eventId, seatId, cancellationToken);
        return NoContent();
    }

    [HttpPost("carts/{cartId}/bookings")]
    public async Task<IActionResult> BookCart(
        string cartId,
        CancellationToken cancellationToken)
    {
        var result = await cartService.BookCartAsync(cartId, cancellationToken);
        return CreatedAtAction(
            nameof(PaymentsController.GetPaymentStatus),
            "Payments",
            new { paymentId = result.PaymentId },
            result);
    }
}
