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

    [HttpPost("carts/{cartId}")]
    public async Task<IActionResult> AddToCart(
        string cartId,
        [FromBody] AddToCartRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var cart = await cartService.AddToCartAsync(cartId, request, cancellationToken);
            return Ok(cart);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("carts/{cartId}/events/{eventId}/seats/{seatId}")]
    public async Task<IActionResult> RemoveFromCart(
        string cartId,
        string eventId,
        string seatId,
        CancellationToken cancellationToken)
    {
        var cart = await cartService.RemoveFromCartAsync(cartId, eventId, seatId, cancellationToken);
        return Ok(cart);
    }

    [HttpPut("carts/{cartId}/book")]
    public async Task<IActionResult> BookCart(
        string cartId,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await cartService.BookCartAsync(cartId, cancellationToken);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
