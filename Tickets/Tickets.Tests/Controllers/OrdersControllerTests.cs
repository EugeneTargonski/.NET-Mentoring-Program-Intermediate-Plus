using Microsoft.AspNetCore.Mvc;
using Moq;
using Tickets.Controllers;
using Tickets.DTOs;
using Tickets.Services.Abstractions;
using Xunit;

namespace Tickets.Tests.Controllers;

public class OrdersControllerTests
{
    private readonly Mock<ICartService> _mockCartService;
    private readonly OrdersController _controller;

    public OrdersControllerTests()
    {
        _mockCartService = new Mock<ICartService>();
        _controller = new OrdersController(_mockCartService.Object);
    }

    [Fact]
    public async Task GetCart_ReturnsOkResult_WithCart()
    {
        // Arrange
        var cartId = "cart-123";
        var expectedCart = new CartDto(
            cartId,
            new List<CartItemDto>
            {
                new CartItemDto("event-1", "seat-1", "price-1", 100m)
            },
            100m);

        _mockCartService
            .Setup(s => s.GetCartAsync(cartId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedCart);

        // Act
        var result = await _controller.GetCart(cartId, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedCart = Assert.IsType<CartDto>(okResult.Value);
        Assert.Equal(cartId, returnedCart.CartId);
        Assert.Single(returnedCart.Items);
        Assert.Equal(100m, returnedCart.TotalAmount);
        _mockCartService.Verify(s => s.GetCartAsync(cartId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetCart_ReturnsOkResult_WithEmptyCart()
    {
        // Arrange
        var cartId = "cart-456";
        var expectedCart = new CartDto(cartId, new List<CartItemDto>(), 0m);

        _mockCartService
            .Setup(s => s.GetCartAsync(cartId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedCart);

        // Act
        var result = await _controller.GetCart(cartId, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedCart = Assert.IsType<CartDto>(okResult.Value);
        Assert.Empty(returnedCart.Items);
        Assert.Equal(0m, returnedCart.TotalAmount);
    }

    [Fact]
    public async Task GetCart_PassesCancellationToken_ToService()
    {
        // Arrange
        var cartId = "cart-123";
        var cancellationToken = new CancellationToken();
        var expectedCart = new CartDto(cartId, new List<CartItemDto>(), 0m);

        _mockCartService
            .Setup(s => s.GetCartAsync(cartId, cancellationToken))
            .ReturnsAsync(expectedCart);

        // Act
        await _controller.GetCart(cartId, cancellationToken);

        // Assert
        _mockCartService.Verify(s => s.GetCartAsync(cartId, cancellationToken), Times.Once);
    }

    [Fact]
    public async Task AddToCart_ReturnsCreatedAtAction_WithCart()
    {
        // Arrange
        var cartId = "cart-123";
        var request = new AddToCartRequest("event-1", "seat-1", "price-1");
        var expectedCart = new CartDto(
            cartId,
            new List<CartItemDto>
            {
                new CartItemDto("event-1", "seat-1", "price-1", 100m)
            },
            100m);

        _mockCartService
            .Setup(s => s.AddToCartAsync(cartId, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedCart);

        // Act
        var result = await _controller.AddToCart(cartId, request, CancellationToken.None);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(OrdersController.GetCart), createdResult.ActionName);
        Assert.Equal(cartId, createdResult.RouteValues?["cartId"]);
        var returnedCart = Assert.IsType<CartDto>(createdResult.Value);
        Assert.Equal(cartId, returnedCart.CartId);
        Assert.Single(returnedCart.Items);
        _mockCartService.Verify(s => s.AddToCartAsync(cartId, request, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddToCart_PassesCorrectParameters_ToService()
    {
        // Arrange
        var cartId = "cart-456";
        var request = new AddToCartRequest("event-2", "seat-2", "price-2");
        var cancellationToken = new CancellationToken();
        var expectedCart = new CartDto(cartId, new List<CartItemDto>(), 0m);

        _mockCartService
            .Setup(s => s.AddToCartAsync(cartId, request, cancellationToken))
            .ReturnsAsync(expectedCart);

        // Act
        await _controller.AddToCart(cartId, request, cancellationToken);

        // Assert
        _mockCartService.Verify(s => s.AddToCartAsync(cartId, request, cancellationToken), Times.Once);
    }

    [Fact]
    public async Task RemoveFromCart_ReturnsNoContent_WhenSuccessful()
    {
        // Arrange
        var cartId = "cart-123";
        var eventId = "event-1";
        var seatId = "seat-1";

        var expectedCart = new CartDto(cartId, new List<CartItemDto>(), 0m);

        _mockCartService
            .Setup(s => s.RemoveFromCartAsync(cartId, eventId, seatId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedCart);

        // Act
        var result = await _controller.RemoveFromCart(cartId, eventId, seatId, CancellationToken.None);

        // Assert
        Assert.IsType<NoContentResult>(result);
        _mockCartService.Verify(s => s.RemoveFromCartAsync(cartId, eventId, seatId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RemoveFromCart_PassesCorrectParameters_ToService()
    {
        // Arrange
        var cartId = "cart-789";
        var eventId = "event-3";
        var seatId = "seat-3";
        var cancellationToken = new CancellationToken();

        var expectedCart = new CartDto(cartId, new List<CartItemDto>(), 0m);

        _mockCartService
            .Setup(s => s.RemoveFromCartAsync(cartId, eventId, seatId, cancellationToken))
            .ReturnsAsync(expectedCart);

        // Act
        await _controller.RemoveFromCart(cartId, eventId, seatId, cancellationToken);

        // Assert
        _mockCartService.Verify(s => s.RemoveFromCartAsync(cartId, eventId, seatId, cancellationToken), Times.Once);
    }

    [Fact]
    public async Task BookCart_ReturnsCreatedAtAction_WithBookingResponse()
    {
        // Arrange
        var cartId = "cart-123";
        var expectedResponse = new BookCartResponse(
            "payment-456",
            150m,
            new List<string> { "seat-1", "seat-2" });

        _mockCartService
            .Setup(s => s.BookCartAsync(cartId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.BookCart(cartId, CancellationToken.None);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(PaymentsController.GetPaymentStatus), createdResult.ActionName);
        Assert.Equal("Payments", createdResult.ControllerName);
        Assert.Equal("payment-456", createdResult.RouteValues?["paymentId"]);

        var returnedResponse = Assert.IsType<BookCartResponse>(createdResult.Value);
        Assert.Equal("payment-456", returnedResponse.PaymentId);
        Assert.Equal(150m, returnedResponse.TotalAmount);
        Assert.Equal(2, returnedResponse.BookedSeats.Count);
        _mockCartService.Verify(s => s.BookCartAsync(cartId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task BookCart_PassesCancellationToken_ToService()
    {
        // Arrange
        var cartId = "cart-123";
        var cancellationToken = new CancellationToken();
        var expectedResponse = new BookCartResponse("payment-456", 150m, new List<string>());

        _mockCartService
            .Setup(s => s.BookCartAsync(cartId, cancellationToken))
            .ReturnsAsync(expectedResponse);

        // Act
        await _controller.BookCart(cartId, cancellationToken);

        // Assert
        _mockCartService.Verify(s => s.BookCartAsync(cartId, cancellationToken), Times.Once);
    }

    [Fact]
    public async Task GetCart_PropagatesException_WhenServiceThrows()
    {
        // Arrange
        var cartId = "cart-123";

        _mockCartService
            .Setup(s => s.GetCartAsync(cartId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException("Cart not found"));

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => 
            _controller.GetCart(cartId, CancellationToken.None));
    }

    [Fact]
    public async Task AddToCart_PropagatesException_WhenServiceThrows()
    {
        // Arrange
        var cartId = "cart-123";
        var request = new AddToCartRequest("event-1", "seat-1", "price-1");

        _mockCartService
            .Setup(s => s.AddToCartAsync(cartId, request, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Seat not available"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _controller.AddToCart(cartId, request, CancellationToken.None));
    }

    [Fact]
    public async Task RemoveFromCart_PropagatesException_WhenServiceThrows()
    {
        // Arrange
        var cartId = "cart-123";
        var eventId = "event-1";
        var seatId = "seat-1";

        _mockCartService
            .Setup(s => s.RemoveFromCartAsync(cartId, eventId, seatId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException("Item not found in cart"));

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => 
            _controller.RemoveFromCart(cartId, eventId, seatId, CancellationToken.None));
    }

    [Fact]
    public async Task BookCart_PropagatesException_WhenServiceThrows()
    {
        // Arrange
        var cartId = "cart-123";

        _mockCartService
            .Setup(s => s.BookCartAsync(cartId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Cart is empty"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _controller.BookCart(cartId, CancellationToken.None));
    }
}
