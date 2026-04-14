using Microsoft.Extensions.Logging;
using Tickets.Data.Abstractions;
using Tickets.Domain.Entities;
using Tickets.Domain.Enums;

namespace Tickets.Demo;

/// <summary>
/// Responsibility: Demonstrate Inventory Service operations (InventoryDb)
/// Now depends on abstraction (IUnitOfWork) instead of implementation (ICosmosUnitOfWork)
/// </summary>
public class InventoryDemoScenarios(IUnitOfWork unitOfWork, ILogger<InventoryDemoScenarios> logger)
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));  // ✅ Depends on abstraction
    private readonly ILogger<InventoryDemoScenarios> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task RunAllAsync()
    {
        await CreateSeatsAsync();
        await GetAvailableSeatsAsync();
        await HoldSeatAsync();
        await ReleaseExpiredHoldsAsync();
    }

    private async Task CreateSeatsAsync()
    {
        _logger.LogInformation("--- Demo: Create Seats (InventoryDb) ---");

        var events = await _unitOfWork.Events.GetAllAsync();
        var firstEvent = events.FirstOrDefault();
        var offers = await _unitOfWork.Offers.GetAllAsync();
        var adultOffer = offers.FirstOrDefault(o => o.PriceCategory == PriceCategory.Adult);

        if (firstEvent == null || adultOffer == null)
        {
            _logger.LogWarning("No events or offers found, skipping seat creation");
            return;
        }

        var seats = Enumerable.Range(1, 20).Select(i => new Seat
        {
            EventId = firstEvent.Id,
            ManifestId = firstEvent.ManifestId,
            SeatNumber = $"A{i}",
            Row = "A",
            Section = "Section 1",
            Status = SeatStatus.Available,
            CurrentOfferId = adultOffer.Id,
            CurrentOffer = new OfferInfo 
            { 
                OfferId = adultOffer.Id, 
                Name = adultOffer.Name, 
                Price = adultOffer.Price, 
                PriceCategory = adultOffer.PriceCategory 
            }
        }).ToList();

        await _unitOfWork.Seats.CreateBulkAsync(seats);
        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("Created {Count} seats in InventoryDb", seats.Count);
        }
    }

    private async Task GetAvailableSeatsAsync()
    {
        _logger.LogInformation("--- Demo: Get Available Seats (InventoryDb) ---");

        var events = await _unitOfWork.Events.GetAllAsync();
        var firstEvent = events.FirstOrDefault();
        
        if (firstEvent == null)
        {
            _logger.LogWarning("No events found");
            return;
        }

        var availableSeats = await _unitOfWork.Seats.GetAvailableSeatsWithOffersByEventIdAsync(firstEvent.Id);
        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("Found {Count} available seats in InventoryDb", availableSeats.Count());
        }
    }

    private async Task HoldSeatAsync()
    {
        _logger.LogInformation("--- Demo: Hold Seat (InventoryDb) ---");

        var events = await _unitOfWork.Events.GetAllAsync();
        var firstEvent = events.FirstOrDefault();
        
        if (firstEvent == null)
        {
            _logger.LogWarning("No events found");
            return;
        }

        var availableSeats = await _unitOfWork.Seats.GetAvailableSeatsByEventIdAsync(firstEvent.Id);
        var seatToHold = availableSeats.FirstOrDefault();
        
        if (seatToHold == null)
        {
            _logger.LogWarning("No available seats to hold");
            return;
        }

        var success = await _unitOfWork.Seats.HoldSeatAsync(
            seatToHold.Id, 
            firstEvent.Id, 
            "customer123", 
            DateTime.UtcNow.AddMinutes(15));
        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("Seat hold {Status} in InventoryDb", success ? "succeeded" : "failed");
        }
    }

    private async Task ReleaseExpiredHoldsAsync()
    {
        _logger.LogInformation("--- Demo: Release Expired Holds (InventoryDb) ---");

        var events = await _unitOfWork.Events.GetAllAsync();
        var firstEvent = events.FirstOrDefault();
        
        if (firstEvent == null)
        {
            _logger.LogWarning("No events found");
            return;
        }

        var count = await _unitOfWork.Seats.ReleaseExpiredHoldsAsync(firstEvent.Id);
        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("Released {Count} expired holds in InventoryDb", count);
        }
    }
}