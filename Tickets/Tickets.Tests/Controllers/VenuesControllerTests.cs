using Microsoft.AspNetCore.Mvc;
using Moq;
using Tickets.Controllers;
using Tickets.DTOs;
using Tickets.Services.Abstractions;
using Xunit;

namespace Tickets.Tests.Controllers;

public class VenuesControllerTests
{
    private readonly Mock<IVenueService> _mockVenueService;
    private readonly VenuesController _controller;

    public VenuesControllerTests()
    {
        _mockVenueService = new Mock<IVenueService>();
        _controller = new VenuesController(_mockVenueService.Object);
    }

    [Fact]
    public async Task GetVenues_ReturnsOkResult_WithListOfVenues()
    {
        // Arrange
        var expectedVenues = new List<VenueDto>
        {
            new VenueDto("venue-1", "Madison Square Garden", "4 Pennsylvania Plaza", "New York", "USA", 20000),
            new VenueDto("venue-2", "Staples Center", "1111 S Figueroa St", "Los Angeles", "USA", 18000)
        };

        _mockVenueService
            .Setup(s => s.GetAllVenuesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedVenues);

        // Act
        var result = await _controller.GetVenues(CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedVenues = Assert.IsAssignableFrom<IEnumerable<VenueDto>>(okResult.Value);
        Assert.Equal(2, returnedVenues.Count());
        _mockVenueService.Verify(s => s.GetAllVenuesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetVenues_ReturnsOkResult_WithEmptyList_WhenNoVenues()
    {
        // Arrange
        var expectedVenues = new List<VenueDto>();

        _mockVenueService
            .Setup(s => s.GetAllVenuesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedVenues);

        // Act
        var result = await _controller.GetVenues(CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedVenues = Assert.IsAssignableFrom<IEnumerable<VenueDto>>(okResult.Value);
        Assert.Empty(returnedVenues);
    }

    [Fact]
    public async Task GetVenues_PassesCancellationToken_ToService()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        var expectedVenues = new List<VenueDto>();

        _mockVenueService
            .Setup(s => s.GetAllVenuesAsync(cancellationToken))
            .ReturnsAsync(expectedVenues);

        // Act
        await _controller.GetVenues(cancellationToken);

        // Assert
        _mockVenueService.Verify(s => s.GetAllVenuesAsync(cancellationToken), Times.Once);
    }

    [Fact]
    public async Task GetVenues_PropagatesException_WhenServiceThrows()
    {
        // Arrange
        _mockVenueService
            .Setup(s => s.GetAllVenuesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _controller.GetVenues(CancellationToken.None));
    }

    [Fact]
    public async Task GetVenueSections_ReturnsOkResult_WithListOfSections()
    {
        // Arrange
        var venueId = "venue-123";
        var expectedSections = new List<VenueSectionDto>
        {
            new VenueSectionDto("Section A", 100),
            new VenueSectionDto("Section B", 150)
        };

        _mockVenueService
            .Setup(s => s.GetVenueSectionsAsync(venueId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedSections);

        // Act
        var result = await _controller.GetVenueSections(venueId, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedSections = Assert.IsAssignableFrom<IEnumerable<VenueSectionDto>>(okResult.Value);
        Assert.Equal(2, returnedSections.Count());
        _mockVenueService.Verify(s => s.GetVenueSectionsAsync(venueId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetVenueSections_ReturnsOkResult_WithEmptyList_WhenNoSections()
    {
        // Arrange
        var venueId = "venue-456";
        var expectedSections = new List<VenueSectionDto>();

        _mockVenueService
            .Setup(s => s.GetVenueSectionsAsync(venueId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedSections);

        // Act
        var result = await _controller.GetVenueSections(venueId, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedSections = Assert.IsAssignableFrom<IEnumerable<VenueSectionDto>>(okResult.Value);
        Assert.Empty(returnedSections);
    }

    [Fact]
    public async Task GetVenueSections_PassesCorrectParameters_ToService()
    {
        // Arrange
        var venueId = "venue-789";
        var cancellationToken = new CancellationToken();
        var expectedSections = new List<VenueSectionDto>();

        _mockVenueService
            .Setup(s => s.GetVenueSectionsAsync(venueId, cancellationToken))
            .ReturnsAsync(expectedSections);

        // Act
        await _controller.GetVenueSections(venueId, cancellationToken);

        // Assert
        _mockVenueService.Verify(s => s.GetVenueSectionsAsync(venueId, cancellationToken), Times.Once);
    }

    [Fact]
    public async Task GetVenueSections_PropagatesException_WhenServiceThrows()
    {
        // Arrange
        var venueId = "venue-123";

        _mockVenueService
            .Setup(s => s.GetVenueSectionsAsync(venueId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException("Venue not found"));

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => 
            _controller.GetVenueSections(venueId, CancellationToken.None));
    }

    [Fact]
    public async Task GetVenues_ReturnsVenuesWithCorrectProperties()
    {
        // Arrange
        var venue = new VenueDto("venue-1", "Test Arena", "123 Test St", "Test City", "Test Country", 15000);
        var expectedVenues = new List<VenueDto> { venue };

        _mockVenueService
            .Setup(s => s.GetAllVenuesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedVenues);

        // Act
        var result = await _controller.GetVenues(CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedVenues = Assert.IsAssignableFrom<IEnumerable<VenueDto>>(okResult.Value);
        var returnedVenue = returnedVenues.First();
        Assert.Equal("venue-1", returnedVenue.Id);
        Assert.Equal("Test Arena", returnedVenue.Name);
        Assert.Equal("123 Test St", returnedVenue.Address);
        Assert.Equal("Test City", returnedVenue.City);
        Assert.Equal("Test Country", returnedVenue.Country);
        Assert.Equal(15000, returnedVenue.Capacity);
    }

    [Fact]
    public async Task GetVenueSections_ReturnsSectionsWithCorrectProperties()
    {
        // Arrange
        var venueId = "venue-123";
        var section = new VenueSectionDto("VIP Section", 50);
        var expectedSections = new List<VenueSectionDto> { section };

        _mockVenueService
            .Setup(s => s.GetVenueSectionsAsync(venueId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedSections);

        // Act
        var result = await _controller.GetVenueSections(venueId, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedSections = Assert.IsAssignableFrom<IEnumerable<VenueSectionDto>>(okResult.Value);
        var returnedSection = returnedSections.First();
        Assert.Equal("VIP Section", returnedSection.Section);
        Assert.Equal(50, returnedSection.SeatCount);
    }
}
