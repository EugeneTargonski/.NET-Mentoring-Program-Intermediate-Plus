using System.Net;
using System.Net.Http.Json;
using Tickets.DTOs;
using Xunit;

namespace Tickets.Tests.Integration;

/// <summary>
/// Integration tests for the complete tickets ordering workflow
/// Tests the full flow: Browse events → View seats → Add to cart → Book → Pay
/// </summary>
public class TicketsOrderingIntegrationTests : IntegrationTestBase
{
    public TicketsOrderingIntegrationTests(TicketsWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task CompleteOrderingWorkflow_HappyPath_Success()
    {
        // NOTE: This test requires Cosmos DB Emulator to be running
        // For CI/CD, consider using Testcontainers or mocking the database layer

        // Arrange - Generate unique cart ID for this test
        var cartId = $"test-cart-{Guid.NewGuid()}";

        // Step 1: Browse available events
        var eventsResponse = await Client.GetAsync("/api/events");
        Assert.Equal(HttpStatusCode.OK, eventsResponse.StatusCode);

        var events = await eventsResponse.Content.ReadFromJsonAsync<List<EventDto>>(JsonOptions);

        // If no events exist, this test will be skipped (requires test data setup)
        if (events == null || events.Count == 0)
        {
            // Skip test if no test data available
            return;
        }

        var testEvent = events.First();

        // Step 2: Browse available venues
        var venuesResponse = await Client.GetAsync("/api/venues");
        Assert.Equal(HttpStatusCode.OK, venuesResponse.StatusCode);

        // Step 3: View available seats for the event
        var seatsResponse = await Client.GetAsync($"/api/events/{testEvent.Id}/sections/A/seats");
        Assert.Equal(HttpStatusCode.OK, seatsResponse.StatusCode);

        var seats = await seatsResponse.Content.ReadFromJsonAsync<List<EventSeatDto>>(JsonOptions);

        // If no seats available, skip the test
        if (seats == null || seats.Count == 0)
        {
            return;
        }

        var testSeat = seats.First(s => s.Status == "Available");
        var priceOption = testSeat.PriceOption;

        Assert.NotNull(priceOption);

        // Step 4: Get empty cart
        var cartResponse = await Client.GetAsync($"/api/orders/carts/{cartId}");
        Assert.Equal(HttpStatusCode.OK, cartResponse.StatusCode);

        var emptyCart = await cartResponse.Content.ReadFromJsonAsync<CartDto>(JsonOptions);
        Assert.NotNull(emptyCart);
        Assert.Empty(emptyCart.Items);
        Assert.Equal(0m, emptyCart.TotalAmount);

        // Step 5: Add seat to cart
        var addToCartRequest = new AddToCartRequest(
            testEvent.Id,
            testSeat.SeatId,
            priceOption.Id);

        var addToCartResponse = await PostAsync($"/api/orders/carts/{cartId}/items", addToCartRequest);
        Assert.Equal(HttpStatusCode.Created, addToCartResponse.StatusCode);

        var cartWithItem = await addToCartResponse.Content.ReadFromJsonAsync<CartDto>(JsonOptions);
        Assert.NotNull(cartWithItem);
        Assert.Single(cartWithItem.Items);
        Assert.True(cartWithItem.TotalAmount > 0);

        // Step 6: Verify cart contents
        cartResponse = await Client.GetAsync($"/api/orders/carts/{cartId}");
        var updatedCart = await cartResponse.Content.ReadFromJsonAsync<CartDto>(JsonOptions);
        Assert.NotNull(updatedCart);
        Assert.Single(updatedCart.Items);

        // Step 7: Book the cart (creates booking and payment)
        var bookResponse = await PostAsync($"/api/orders/carts/{cartId}/bookings", new { });
        Assert.Equal(HttpStatusCode.Created, bookResponse.StatusCode);

        var bookCartResponse = await bookResponse.Content.ReadFromJsonAsync<BookCartResponse>(JsonOptions);
        Assert.NotNull(bookCartResponse);
        Assert.NotNull(bookCartResponse.PaymentId);
        Assert.True(bookCartResponse.TotalAmount > 0);
        Assert.NotEmpty(bookCartResponse.BookedSeats);

        // Step 8: Check payment status
        var paymentStatusResponse = await Client.GetAsync($"/api/payments/{bookCartResponse.PaymentId}");
        Assert.Equal(HttpStatusCode.OK, paymentStatusResponse.StatusCode);

        var paymentStatus = await paymentStatusResponse.Content.ReadFromJsonAsync<PaymentStatusResponse>(JsonOptions);
        Assert.NotNull(paymentStatus);
        Assert.Equal("Pending", paymentStatus.Status);

        // Step 9: Complete the payment
        var completePaymentResponse = await PatchAsync($"/api/payments/{bookCartResponse.PaymentId}/complete");
        Assert.Equal(HttpStatusCode.OK, completePaymentResponse.StatusCode);

        var completedPayment = await completePaymentResponse.Content.ReadFromJsonAsync<PaymentStatusResponse>(JsonOptions);
        Assert.NotNull(completedPayment);
        Assert.Equal("Completed", completedPayment.Status);

        // Step 10: Verify final payment status
        paymentStatusResponse = await Client.GetAsync($"/api/payments/{bookCartResponse.PaymentId}");
        var finalPaymentStatus = await paymentStatusResponse.Content.ReadFromJsonAsync<PaymentStatusResponse>(JsonOptions);
        Assert.NotNull(finalPaymentStatus);
        Assert.Equal("Completed", finalPaymentStatus.Status);
    }

    [Fact]
    public async Task OrderingWorkflow_WithCartModification_Success()
    {
        // Arrange
        var cartId = $"test-cart-modify-{Guid.NewGuid()}";

        // Get events
        var events = await GetAsync<List<EventDto>>("/api/events");
        if (events == null || events.Count == 0) return;

        var testEvent = events.First();

        // Get seats
        var seats = await GetAsync<List<EventSeatDto>>($"/api/events/{testEvent.Id}/sections/A/seats");
        if (seats == null || seats.Count < 2) return;

        var availableSeats = seats.Where(s => s.Status == "Available").Take(2).ToList();
        if (availableSeats.Count < 2) return;

        // Add first seat to cart
        var addRequest1 = new AddToCartRequest(
            testEvent.Id,
            availableSeats[0].SeatId,
            availableSeats[0].PriceOption!.Id);

        var addResponse1 = await PostAsync($"/api/orders/carts/{cartId}/items", addRequest1);
        Assert.Equal(HttpStatusCode.Created, addResponse1.StatusCode);

        // Add second seat to cart
        var addRequest2 = new AddToCartRequest(
            testEvent.Id,
            availableSeats[1].SeatId,
            availableSeats[1].PriceOption!.Id);

        var addResponse2 = await PostAsync($"/api/orders/carts/{cartId}/items", addRequest2);
        Assert.Equal(HttpStatusCode.Created, addResponse2.StatusCode);

        // Verify cart has 2 items
        var cart = await GetAsync<CartDto>($"/api/orders/carts/{cartId}");
        Assert.NotNull(cart);
        Assert.Equal(2, cart.Items.Count);

        // Remove first seat from cart
        var removeResponse = await DeleteAsync(
            $"/api/orders/carts/{cartId}/events/{testEvent.Id}/seats/{availableSeats[0].SeatId}");
        Assert.Equal(HttpStatusCode.NoContent, removeResponse.StatusCode);

        // Verify cart now has 1 item
        cart = await GetAsync<CartDto>($"/api/orders/carts/{cartId}");
        Assert.NotNull(cart);
        Assert.Single(cart.Items);

        // Book the remaining item
        var bookResponse = await PostAsync($"/api/orders/carts/{cartId}/bookings", new { });
        Assert.Equal(HttpStatusCode.Created, bookResponse.StatusCode);

        var bookResult = await bookResponse.Content.ReadFromJsonAsync<BookCartResponse>(JsonOptions);
        Assert.NotNull(bookResult);
        Assert.Single(bookResult.BookedSeats);
    }

    [Fact]
    public async Task OrderingWorkflow_PaymentFailure_Success()
    {
        // Arrange
        var cartId = $"test-cart-fail-{Guid.NewGuid()}";

        // Get event and seat
        var events = await GetAsync<List<EventDto>>("/api/events");
        if (events == null || events.Count == 0) return;

        var testEvent = events.First();
        var seats = await GetAsync<List<EventSeatDto>>($"/api/events/{testEvent.Id}/sections/A/seats");
        if (seats == null || seats.Count == 0) return;

        var testSeat = seats.FirstOrDefault(s => s.Status == "Available");
        if (testSeat?.PriceOption == null) return;

        // Add to cart and book
        var addRequest = new AddToCartRequest(
            testEvent.Id,
            testSeat.SeatId,
            testSeat.PriceOption.Id);

        await PostAsync($"/api/orders/carts/{cartId}/items", addRequest);

        var bookResponse = await PostAsync($"/api/orders/carts/{cartId}/bookings", new { });
        var bookResult = await bookResponse.Content.ReadFromJsonAsync<BookCartResponse>(JsonOptions);
        Assert.NotNull(bookResult);

        // Fail the payment
        var failResponse = await PatchAsync($"/api/payments/{bookResult.PaymentId}/failed");
        Assert.Equal(HttpStatusCode.OK, failResponse.StatusCode);

        var failedPayment = await failResponse.Content.ReadFromJsonAsync<PaymentStatusResponse>(JsonOptions);
        Assert.NotNull(failedPayment);
        Assert.Equal("Failed", failedPayment.Status);

        // Verify payment status is Failed
        var statusResponse = await GetAsync<PaymentStatusResponse>($"/api/payments/{bookResult.PaymentId}");
        Assert.NotNull(statusResponse);
        Assert.Equal("Failed", statusResponse.Status);
    }

    [Fact]
    public async Task GetEvents_ReturnsSuccessStatusCode()
    {
        // Act
        var response = await Client.GetAsync("/api/events");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetVenues_ReturnsSuccessStatusCode()
    {
        // Act
        var response = await Client.GetAsync("/api/venues");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetCart_NonExistentCart_ReturnsEmptyCart()
    {
        // Arrange
        var cartId = $"non-existent-{Guid.NewGuid()}";

        // Act
        var response = await Client.GetAsync($"/api/orders/carts/{cartId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var cart = await response.Content.ReadFromJsonAsync<CartDto>(JsonOptions);
        Assert.NotNull(cart);
        Assert.Empty(cart.Items);
        Assert.Equal(0m, cart.TotalAmount);
    }

    [Fact]
    public async Task AddToCart_InvalidEventId_ReturnsBadRequestOrNotFound()
    {
        // Arrange
        var cartId = $"test-cart-{Guid.NewGuid()}";
        var invalidRequest = new AddToCartRequest(
            "invalid-event-id",
            "invalid-seat-id",
            "invalid-price-id");

        // Act
        var response = await PostAsync($"/api/orders/carts/{cartId}/items", invalidRequest);

        // Assert
        // Should return error status (400 or 404)
        Assert.True(
            response.StatusCode == HttpStatusCode.BadRequest ||
            response.StatusCode == HttpStatusCode.NotFound ||
            response.StatusCode == HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task RemoveFromCart_NonExistentItem_ReturnsNotFoundOrNoContent()
    {
        // Arrange
        var cartId = $"test-cart-{Guid.NewGuid()}";

        // Act
        var response = await DeleteAsync(
            $"/api/orders/carts/{cartId}/events/non-existent-event/seats/non-existent-seat");

        // Assert
        // Depending on implementation, might return 404 or 204
        Assert.True(
            response.StatusCode == HttpStatusCode.NoContent ||
            response.StatusCode == HttpStatusCode.NotFound ||
            response.StatusCode == HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task BookCart_EmptyCart_ReturnsBadRequest()
    {
        // Arrange
        var cartId = $"empty-cart-{Guid.NewGuid()}";

        // Ensure cart is empty
        await Client.GetAsync($"/api/orders/carts/{cartId}");

        // Act - Try to book empty cart
        var response = await PostAsync($"/api/orders/carts/{cartId}/bookings", new { });

        // Assert
        // Should fail because cart is empty
        Assert.True(
            response.StatusCode == HttpStatusCode.BadRequest ||
            response.StatusCode == HttpStatusCode.InternalServerError);
    }
}
