using Microsoft.AspNetCore.Mvc;
using Moq;
using Tickets.Controllers;
using Tickets.DTOs;
using Tickets.Services.Abstractions;
using Xunit;

namespace Tickets.Tests.Controllers;

public class EventsControllerTests
{
    private readonly Mock<IEventService> _mockEventService;
    private readonly EventsController _controller;

    public EventsControllerTests()
    {
        _mockEventService = new Mock<IEventService>();
        _controller = new EventsController(_mockEventService.Object);
    }

    [Fact]
    public async Task GetEvents_ReturnsOkResult_WithListOfEvents()
    {
        // Arrange
        var expectedEvents = new List<EventDto>
        {
            new EventDto("event-1", "Concert", "Rock Concert", DateTime.UtcNow.AddDays(30), null, "venue-1", "Music", true),
            new EventDto("event-2", "Basketball Game", "NBA Game", DateTime.UtcNow.AddDays(15), null, "venue-2", "Sports", true)
        };

        _mockEventService
            .Setup(s => s.GetAllEventsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedEvents);

        // Act
        var result = await _controller.GetEvents(CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedEvents = Assert.IsAssignableFrom<IEnumerable<EventDto>>(okResult.Value);
        Assert.Equal(2, returnedEvents.Count());
        _mockEventService.Verify(s => s.GetAllEventsAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetEvents_ReturnsOkResult_WithEmptyList_WhenNoEvents()
    {
        // Arrange
        var expectedEvents = new List<EventDto>();

        _mockEventService
            .Setup(s => s.GetAllEventsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedEvents);

        // Act
        var result = await _controller.GetEvents(CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedEvents = Assert.IsAssignableFrom<IEnumerable<EventDto>>(okResult.Value);
        Assert.Empty(returnedEvents);
    }

    [Fact]
    public async Task GetEvents_CallsServiceWithCancellationToken()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        var expectedEvents = new List<EventDto>();

        _mockEventService
            .Setup(s => s.GetAllEventsAsync(cancellationToken))
            .ReturnsAsync(expectedEvents);

        // Act
        await _controller.GetEvents(cancellationToken);

        // Assert
        _mockEventService.Verify(s => s.GetAllEventsAsync(cancellationToken), Times.Once);
    }

    [Fact]
    public async Task GetEventSeats_ReturnsOkResult_WithListOfSeats()
    {
        // Arrange
        var eventId = "event-123";
        var sectionId = "section-A";
        var expectedSeats = new List<EventSeatDto>
        {
            new EventSeatDto("seat-1", "section-A", "1", "10", "Available", new PriceOptionDto("price-1", "Adult", 100m)),
            new EventSeatDto("seat-2", "section-A", "1", "11", "Available", new PriceOptionDto("price-2", "Adult", 100m))
        };

        _mockEventService
            .Setup(s => s.GetEventSeatsAsync(eventId, sectionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedSeats);

        // Act
        var result = await _controller.GetEventSeats(eventId, sectionId, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedSeats = Assert.IsAssignableFrom<IEnumerable<EventSeatDto>>(okResult.Value);
        Assert.Equal(2, returnedSeats.Count());
        _mockEventService.Verify(s => s.GetEventSeatsAsync(eventId, sectionId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetEventSeats_ReturnsOkResult_WithEmptyList_WhenNoSeatsAvailable()
    {
        // Arrange
        var eventId = "event-123";
        var sectionId = "section-A";
        var expectedSeats = new List<EventSeatDto>();

        _mockEventService
            .Setup(s => s.GetEventSeatsAsync(eventId, sectionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedSeats);

        // Act
        var result = await _controller.GetEventSeats(eventId, sectionId, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedSeats = Assert.IsAssignableFrom<IEnumerable<EventSeatDto>>(okResult.Value);
        Assert.Empty(returnedSeats);
    }

    [Fact]
    public async Task GetEventSeats_PassesCorrectParameters_ToService()
    {
        // Arrange
        var eventId = "event-456";
        var sectionId = "section-B";
        var cancellationToken = new CancellationToken();
        var expectedSeats = new List<EventSeatDto>();

        _mockEventService
            .Setup(s => s.GetEventSeatsAsync(eventId, sectionId, cancellationToken))
            .ReturnsAsync(expectedSeats);

        // Act
        await _controller.GetEventSeats(eventId, sectionId, cancellationToken);

        // Assert
        _mockEventService.Verify(s => s.GetEventSeatsAsync(eventId, sectionId, cancellationToken), Times.Once);
    }

    [Fact]
    public async Task GetEvents_PropagatesException_WhenServiceThrows()
    {
        // Arrange
        _mockEventService
            .Setup(s => s.GetAllEventsAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _controller.GetEvents(CancellationToken.None));
    }

    [Fact]
    public async Task GetEventSeats_PropagatesException_WhenServiceThrows()
    {
        // Arrange
        var eventId = "event-123";
        var sectionId = "section-A";

        _mockEventService
            .Setup(s => s.GetEventSeatsAsync(eventId, sectionId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException("Event not found"));

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => 
            _controller.GetEventSeats(eventId, sectionId, CancellationToken.None));
    }
}
